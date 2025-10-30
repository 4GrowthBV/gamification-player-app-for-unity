using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace GamificationPlayer.Editor
{
    /// <summary>
    /// UI implementation for the Chat Profiles tab
    /// Handles displaying, creating, editing, and managing chat profiles
    /// </summary>
    public class ChatProfilesTab
    {
        #region Private Fields
        
        private ChatDataCache dataCache;
        private ChatDatabaseAPI api;
        private Vector2 scrollPosition;
        private string searchTerm = "";
        private ChatProfileData selectedProfile;
        private bool showCreateForm = false;
        private bool showEditForm = false;
        
        // Form fields
        private string formName = "";
        private string formUserId = "";
        
        #endregion
        
        #region Constructor
        
        public ChatProfilesTab(ChatDataCache cache, ChatDatabaseAPI databaseApi)
        {
            dataCache = cache;
            api = databaseApi;
        }
        
        #endregion
        
        #region Public Methods
        
        public void OnGUI()
        {
            GUILayout.BeginVertical();
            
            DrawToolbar();
            DrawSearchBar();
            DrawProfilesList();
            DrawDetailsPanel();
            
            GUILayout.EndVertical();
        }
        
        #endregion
        
        #region Private Methods - UI Drawing
        
        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            
            if (GUILayout.Button("Create New Profile", EditorStyles.toolbarButton))
            {
                StartCreateProfile();
            }
            
            if (GUILayout.Button("Refresh", EditorStyles.toolbarButton))
            {
                RefreshProfiles();
            }
            
            GUILayout.FlexibleSpace();
            
            if (selectedProfile != null)
            {
                if (GUILayout.Button("Edit", EditorStyles.toolbarButton))
                {
                    StartEditProfile();
                }
                
                if (GUILayout.Button("Delete", EditorStyles.toolbarButton))
                {
                    if (EditorUtility.DisplayDialog("Delete Profile",
                        $"Are you sure you want to delete profile '{selectedProfile.name}'?", "Delete", "Cancel"))
                    {
                        DeleteProfile();
                    }
                }
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawSearchBar()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Search:", GUILayout.Width(50));
            string newSearchTerm = EditorGUILayout.TextField(searchTerm);
            if (newSearchTerm != searchTerm)
            {
                searchTerm = newSearchTerm;
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
        }
        
        private void DrawProfilesList()
        {
            var profiles = dataCache.SearchProfiles(searchTerm);
            
            EditorGUILayout.LabelField($"Profiles ({profiles.Count})", EditorStyles.boldLabel);
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(200));
            
            foreach (var profile in profiles)
            {
                DrawProfileItem(profile);
            }
            
            EditorGUILayout.EndScrollView();
        }
        
        private void DrawProfileItem(ChatProfileData profile)
        {
            bool isSelected = selectedProfile == profile;
            
            EditorGUILayout.BeginHorizontal(isSelected ? EditorStyles.helpBox : GUIStyle.none);
            
            if (GUILayout.Button("", GUIStyle.none, GUILayout.ExpandWidth(true)))
            {
                selectedProfile = profile;
            }
            
            GUILayout.BeginVertical();
            
            // Profile name with status indicator
            string statusIcon = GetStatusIcon(profile.status);
            EditorGUILayout.LabelField($"{statusIcon} {profile.name}", EditorStyles.boldLabel);
            
            // User ID
            EditorGUILayout.LabelField($"User ID: {profile.userId}", EditorStyles.miniLabel);
            
            // Conversation count
            var conversations = dataCache.GetConversationsForProfile(profile.id);
            EditorGUILayout.LabelField($"Conversations: {conversations.Count}", EditorStyles.miniLabel);
            
            GUILayout.EndVertical();
            
            EditorGUILayout.EndHorizontal();
            
            if (isSelected)
            {
                EditorGUILayout.Space();
            }
        }
        
        private void DrawDetailsPanel()
        {
            if (showCreateForm)
            {
                DrawCreateForm();
            }
            else if (showEditForm && selectedProfile != null)
            {
                DrawEditForm();
            }
            else if (selectedProfile != null)
            {
                DrawProfileDetails();
            }
            else
            {
                EditorGUILayout.HelpBox("Select a profile to view details", MessageType.Info);
            }
        }
        
        private void DrawProfileDetails()
        {
            EditorGUILayout.LabelField("Profile Details", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.LabelField("ID:", selectedProfile.id);
            EditorGUILayout.LabelField("Name:", selectedProfile.name);
            EditorGUILayout.LabelField("User ID:", selectedProfile.userId);
            EditorGUILayout.LabelField("Status:", selectedProfile.status);
            EditorGUILayout.LabelField("Created:", selectedProfile.createdAt.ToString("yyyy-MM-dd HH:mm:ss"));
            EditorGUILayout.LabelField("Modified:", selectedProfile.lastModified.ToString("yyyy-MM-dd HH:mm:ss"));
            
            EditorGUILayout.EndVertical();
            
            // Show conversations
            var conversations = dataCache.GetConversationsForProfile(selectedProfile.id);
            EditorGUILayout.LabelField($"Conversations ({conversations.Count})", EditorStyles.boldLabel);
            
            if (conversations.Count > 0)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                foreach (var conversation in conversations.Take(5)) // Show first 5
                {
                    var messageCount = dataCache.GetMessagesForConversation(conversation.id).Count;
                    EditorGUILayout.LabelField($"• Conversation {conversation.id.Substring(0, 8)}... ({messageCount} messages)");
                }
                
                if (conversations.Count > 5)
                {
                    EditorGUILayout.LabelField($"... and {conversations.Count - 5} more");
                }
                EditorGUILayout.EndVertical();
            }
        }
        
        private void DrawCreateForm()
        {
            EditorGUILayout.LabelField("Create New Profile", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            formName = EditorGUILayout.TextField("Profile Name:", formName);
            formUserId = EditorGUILayout.TextField("User ID:", formUserId);
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Create"))
            {
                CreateProfile();
            }
            
            if (GUILayout.Button("Cancel"))
            {
                CancelCreateProfile();
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawEditForm()
        {
            EditorGUILayout.LabelField("Edit Profile", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            formName = EditorGUILayout.TextField("Profile Name:", formName);
            formUserId = EditorGUILayout.TextField("User ID:", formUserId);
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Save"))
            {
                SaveProfile();
            }
            
            if (GUILayout.Button("Cancel"))
            {
                CancelEditProfile();
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
        }
        
        #endregion
        
        #region Private Methods - Actions
        
        private void StartCreateProfile()
        {
            showCreateForm = true;
            showEditForm = false;
            formName = "";
            formUserId = "";
        }
        
        private void CreateProfile()
        {
            if (string.IsNullOrEmpty(formName) || string.IsNullOrEmpty(formUserId))
            {
                EditorUtility.DisplayDialog("Error", "Please fill in all fields", "OK");
                return;
            }
            
            var newProfile = new ChatProfileData(formName, formUserId);
            dataCache.AddProfile(newProfile);
            
            // Sync with server
            api.CreateProfile(newProfile, (success, error) =>
            {
                if (success)
                {
                    newProfile.status = "new"; // Local creation only for now
                    Debug.Log($"Profile '{newProfile.name}' created successfully");
                }
                else
                {
                    newProfile.status = "error";
                    Debug.LogError($"Failed to create profile: {error}");
                }
            });
            
            selectedProfile = newProfile;
            CancelCreateProfile();
        }
        
        private void StartEditProfile()
        {
            if (selectedProfile == null) return;
            
            showEditForm = true;
            showCreateForm = false;
            formName = selectedProfile.name;
            formUserId = selectedProfile.userId;
        }
        
        private void SaveProfile()
        {
            if (selectedProfile == null) return;
            
            if (string.IsNullOrEmpty(formName) || string.IsNullOrEmpty(formUserId))
            {
                EditorUtility.DisplayDialog("Error", "Please fill in all fields", "OK");
                return;
            }
            
            selectedProfile.name = formName;
            selectedProfile.userId = formUserId;
            dataCache.UpdateProfile(selectedProfile);
            
            // Sync with server
            api.UpdateProfile(selectedProfile, (success, error) =>
            {
                if (success)
                {
                    selectedProfile.status = "modified"; // Local update only for now
                    Debug.Log($"Profile '{selectedProfile.name}' updated successfully");
                }
                else
                {
                    selectedProfile.status = "error";
                    Debug.LogError($"Failed to update profile: {error}");
                }
            });
            
            CancelEditProfile();
        }
        
        private void DeleteProfile()
        {
            if (selectedProfile == null) return;
            
            string profileId = selectedProfile.id;
            string profileName = selectedProfile.name;
            
            // Delete from server first
            api.DeleteProfile(profileId, (success, error) =>
            {
                if (success)
                {
                    dataCache.RemoveProfile(profileId);
                    Debug.Log($"Profile '{profileName}' deleted successfully");
                }
                else
                {
                    Debug.LogError($"Failed to delete profile: {error}");
                }
            });
            
            selectedProfile = null;
        }
        
        private void RefreshProfiles()
        {
            api.GetAllProfiles((profiles, error) =>
            {
                if (profiles != null)
                {
                    Debug.Log($"Refreshed {profiles.Count} profiles from cache");
                }
                else
                {
                    Debug.LogError($"Failed to refresh profiles: {error}");
                }
            });
        }
        
        private void CancelCreateProfile()
        {
            showCreateForm = false;
            formName = "";
            formUserId = "";
        }
        
        private void CancelEditProfile()
        {
            showEditForm = false;
            formName = "";
            formUserId = "";
        }
        
        #endregion
        
        #region Private Methods - Utilities
        
        private string GetStatusIcon(string status)
        {
            switch (status)
            {
                case "synced": return "✓";
                case "modified": return "●";
                case "new": return "+";
                case "error": return "⚠";
                default: return "○";
            }
        }
        
        #endregion
    }
}