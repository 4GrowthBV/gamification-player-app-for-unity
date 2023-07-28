using System.Collections;
using System.Collections.Generic;
using GamificationPlayer.DTO.ModuleSession;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace GamificationPlayer.Tests
{
    public class GetModuleSessionsResponseDTOTest
    {
        [Test]
        public void Test()
        {
            GamificationPlayerConfig.TryGetEnvironmentConfig(".it", out var gamificationPlayerEnvironmentConfig);

            gamificationPlayerEnvironmentConfig.TryGetMockDTO<GetModuleSessionsResponseDTO>(out var dto);

            Assert.NotNull(dto);

            Assert.NotNull(dto.data);

            Assert.That(!string.IsNullOrEmpty(dto.data[0].id));

            Assert.AreEqual(dto.data[0].Type, "module_session");

            Assert.NotNull(dto.data[0].attributes);

            Assert.NotNull(dto.data[0].relationships);

            Assert.That(!string.IsNullOrEmpty(dto.data[0].attributes.started_at));
            Assert.That(!string.IsNullOrEmpty(dto.data[0].attributes.ended_at));
            Assert.That(!string.IsNullOrEmpty(dto.data[0].attributes.completed_at));

            Assert.That(dto.data[0].attributes.started_at != default);
            Assert.That(dto.data[0].attributes.ended_at != default);
            Assert.That(dto.data[0].attributes.completed_at != default);

            Assert.AreEqual(dto.data[0].relationships.challenge_session.data.type, "challenge_session");
            Assert.That(!string.IsNullOrEmpty(dto.data[0].relationships.challenge_session.data.id));

            Assert.AreEqual(dto.data[0].relationships.module.data.type, "module");
            Assert.That(!string.IsNullOrEmpty(dto.data[0].relationships.module.data.id));
        }

        [Test]
        public void TestJSON()
        {
            //Test dictionary
            string jsonString = @"{
                ""data"": [{
                    ""id"": ""497f6eca-6276-4993-bfeb-53cbbbba6f08"",
                    ""type"": ""module_session"",
                    ""attributes"": {
                        ""started_at"": ""2019-08-24T14:15:22Z"",
                        ""ended_at"": ""2019-08-24T14:15:22Z"",
                        ""completed_at"": ""2019-08-24T14:15:22Z"",
                        ""score"": 0,
                        ""extra_data"": {
                            ""eee"": ""2"",
                            ""test2"": ""test2"",
                            ""test"": ""test""
                        }
                    },
                    ""relationships"": {
                        ""challenge_session"": {
                            ""data"": {
                                ""id"": ""497f6eca-6276-4993-bfeb-53cbbbba6f08"",
                                ""type"": ""challenge_session""
                            }
                        },
                        ""module"": {
                            ""data"": {
                                ""id"": ""497f6eca-6276-4993-bfeb-53cbbbba6f08"",
                                ""type"": ""module""
                            }
                        }
                    }
                }]
            }";

            var obj = jsonString.FromJson<GetModuleSessionsResponseDTO>();

            Assert.NotNull(obj);

            Assert.NotNull(obj.data);

            Assert.That(!string.IsNullOrEmpty(obj.data[0].id));

            Assert.AreEqual(obj.data[0].Type, "module_session");

            Assert.NotNull(obj.data[0].attributes.extra_data);

            Debug.Log(string.Join(", ", obj.data[0].attributes.extra_data.Values));
            Debug.Log(string.Join(", ", obj.data[0].attributes.extra_data.Keys));

            Assert.AreEqual(obj.data[0].attributes.extra_data["eee"], "2");
            Assert.AreEqual(obj.data[0].attributes.extra_data["test2"], "test2");
            Assert.AreEqual(obj.data[0].attributes.extra_data["test"], "test");

            Assert.NotNull(obj.data[0].attributes);

            Assert.NotNull(obj.data[0].relationships);

            Assert.That(!string.IsNullOrEmpty(obj.data[0].attributes.started_at));
            Assert.That(!string.IsNullOrEmpty(obj.data[0].attributes.ended_at));
            Assert.That(!string.IsNullOrEmpty(obj.data[0].attributes.completed_at));

            Assert.That(obj.data[0].attributes.started_at != default);
            Assert.That(obj.data[0].attributes.ended_at != default);
            Assert.That(obj.data[0].attributes.completed_at != default);

            Assert.AreEqual(obj.data[0].relationships.challenge_session.data.type, "challenge_session");
            Assert.That(!string.IsNullOrEmpty(obj.data[0].relationships.challenge_session.data.id));

            Assert.AreEqual(obj.data[0].relationships.module.data.type, "module");
            Assert.That(!string.IsNullOrEmpty(obj.data[0].relationships.module.data.id));
        }
    }
}
