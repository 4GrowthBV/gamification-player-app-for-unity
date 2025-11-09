mergeInto(LibraryManager.library, {
  JS_StartOpenAIStream: function (apiKeyPtr, bodyPtr) {
    var apiKey = UTF8ToString(apiKeyPtr);
    var body = UTF8ToString(bodyPtr);
    var url = "https://api.openai.com/v1/chat/completions";

    fetch(url, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        "Authorization": "Bearer " + apiKey
      },
      body: body
    })
    .then(function (response) {
      var reader = response.body.getReader();
      var decoder = new TextDecoder();
      var buffer = "";
      var finalText = "";

      function process(result) {
        if (result.done) {
          // send accumulated text to Unity
          SendMessage("WebGLChatBridge", "OnStreamCompleteJS", finalText);
          return;
        }

        buffer += decoder.decode(result.value, { stream: true });
        var parts = buffer.split("\n\n");
        buffer = parts.pop();

        for (var i = 0; i < parts.length; i++) {
          var part = parts[i];
          if (part.indexOf("data: ") === 0) {
            var data = part.substring(6).trim();

            if (data === "[DONE]") {
              // stream finished — send full text
              SendMessage("WebGLChatBridge", "OnStreamCompleteJS", finalText);
              return;
            }

            try {
              var json = JSON.parse(data);
              var choices = json && json.choices ? json.choices : null;
              if (choices && choices.length > 0) {
                var delta = choices[0].delta ? choices[0].delta.content : null;
                if (delta) {
                  finalText += delta; // accumulate
                  SendMessage("WebGLChatBridge", "OnStreamChunkJS", delta);
                }
              }
            } catch (e) {
              // ignore malformed chunks
            }
          }
        }

        reader.read().then(process);
      }

      reader.read().then(process);
    })
    .catch(function (err) {
      SendMessage("WebGLChatBridge", "OnStreamCompleteJS", "[ERROR] " + err.message);
    });
  }
});