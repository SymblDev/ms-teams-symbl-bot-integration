// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ManagementPageController.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// </copyright>
// <summary>
//   JoinCallController is a third-party controller (non-Bot Framework) that can be called in CVI scenario to trigger the bot to join a call
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PsiBot.Services.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using PsiBot.Model.Constants;
    using System.Net.Http;
    using System.Text;

    /// <summary>
    /// Management page allowing joining, listing, and leaving calls.
    /// </summary>
    public class ManagementPageController : ControllerBase
    {
        /// <summary>
        /// The join call async.
        /// </summary>
        /// <returns>
        /// The <see cref="HttpResponseMessage"/>.
        /// </returns>
        [HttpGet]
        [Route(HttpRouteConstants.Management + "/")]
        public ContentResult ManagementPage()
        {
            var html =
                @"<!DOCTYPE html>
                  <html lang='en'>
                      <head>
                            <script src=""https://cdnjs.cloudflare.com/ajax/libs/jquery/3.6.0/jquery.min.js""
                                integrity=""sha512-894YE6QWD5I59HgZOGReFYm4dnWc1Qt5NtvYSaNcOP+u1T9qYdvdihz0PPSiiqn/+/3e7Jo4EaG7TubfWGUrMQ==""
                                crossorigin=""anonymous"" referrerpolicy=""no-referrer""></script>
                            <script type=""text/javascript"">
                                  function SubscribeToWebSocket(url, callId)
                                  {
                                        if (""WebSocket"" in window) 
                                        {
                                            // Let us open a web socket
                                            var ws = new WebSocket(url);

                                            wsSubscriberDict[callId] = ws;

                                            ws.onmessage = function(event) {

                                                var received_msg = event.data;
                                                console.log(received_msg);
                                                const jsonResponse = JSON.parse(received_msg);

                                                if (jsonResponse.type === 'message' && jsonResponse.message.type === 'conversation_completed') {
                                                    console.log('conversationId: '+ jsonResponse.message.conversationId);
                                                    document.getElementById('conversationId'+callId).innerHTML = 'Conversation Id: '+ jsonResponse.message.conversationId;
                                                }

                                                if(jsonResponse.type === 'message' && jsonResponse.message.type === 'recognition_result')
                                                {
                                                    document.getElementById('speaker'+callId).innerHTML = 'Speaker: '+ jsonResponse.message.user.name;
                                                    document.getElementById('cc-text'+callId).innerHTML = jsonResponse.message.punctuated.transcript;
                                                    scrollToBottom('cc-text'+callId);
                                                }
                                               
                                            };

                                            ws.onclose = function() {
                                                // websocket is closed.
                                                console.log(""Connection is closed..."");
                                            };
                                        }
                                        else
                                        {
                                            // The browser doesn't support WebSocket
                                            console.log(""WebSocket NOT supported by your Browser!"");
                                        }
                                }
                              </script>
                         <link rel=""stylesheet"" 
                            href=""https://cdn.jsdelivr.net/npm/bootstrap@5.1.3/dist/css/bootstrap.min.css""
                            integrity=""sha384-1BmE4kWBq78iYhFldvKuhfTAU6auU8tT94WrHftjDbrCEXSU1oBoqyl2QvZ6jIW3"" crossorigin=""anonymous"" />
                          <meta charset='UTF-8'>
                          <title>Teams Bot</title>
                          <style>
                                body {
                                    -ms-overflow-style: none; /* for Internet Explorer, Edge */
                                    scrollbar-width: none; /* for Firefox */
                                    overflow-y: scroll; 
                                    overflow-y: auto;
                                    color: black;
                                    background: white;
                                    text-align: center;
                                    font-family: -apple-system,BlinkMacSystemFont,""Segoe UI"",Roboto,""Helvetica Neue"",Arial,sans-serif,""Apple Color Emoji"",""Segoe UI Emoji"",""Segoe UI Symbol"";
                                }

                                .cc-text::-webkit-scrollbar {
                                  display: none; /* for Chrome, Safari, and Opera */
                                }

                                .cc-text::-webkit-scrollbar {
                                    width: 0px;
                                    background: transparent; /* make scrollbar transparent */
                                }

                                table { margin: auto; }
                                td { padding: 0.5em; }

                                .speaker {
                                    display: inline-block;
                                    margin: 0 auto;
                                    background: green;
                                    color: white;
                                    font-size: 1.5em;
                                    font-family:monospace;
                                    width: 100%;
                                    float: left;
                                }

                                /* Caption Styling */
                                .cc {
                                    text-align: center;
                                    width: 100%;  
                                    left: 10%;
                                    top: 1em;
                                }

                                .cc-text {
                                    display: inline-block;
                                    width: 100%;
                                    margin: 0 auto;
                                    background: black;
                                    color: white;
                                    font-size: 1.5em;
                                    font-family:monospace;
                                    padding: 0.5em;
                                    min-height: 1.5em;
                                    line-height: 2em;
                                    overflow: scroll;
                                    overflow-y: auto;
                                    scrollbar-width: thin;
                                    height: 100px;
                                }
                          </style>
                          <script language='javascript'>
                              var wsSubscriberDict = {};
                              function api(method, path, payload, callback) {
                                  var xhr = new XMLHttpRequest();
                                  xhr.onreadystatechange = function () {
                                      if (this.readyState == 4) {
                                          callback(this.responseText);
                                      }
                                  };
                                  xhr.open(method, document.location.protocol + '//' + document.location.host + '/' + path, true);
                                  if (payload) {
                                      xhr.setRequestHeader('Content-type', 'application/json');
                                      xhr.send(JSON.stringify(payload));
                                  }
                                  else {
                                      try
                                      {
                                         xhr.send();
                                      }
                                      catch(err) {
                                        console.log(err);
                                      }
                                  }
                              }
                              function join(meetingUrl) {
                                  api('POST', 'joinCall', { JoinURL: meetingUrl }, _ => { updateCalls(); });
                              }
                              function leave(legId) {
                                  api('DELETE', 'calls/' + legId, null, _ => { window.setTimeout(updateCalls, 2000); });
                              }
                              function scrollToBottom (id) {
                                   var div = document.getElementById(id);
                                   $('#' + id).animate({
                                      scrollTop: div.scrollHeight
                                   }, 500);
                              }
                              function getSubscribeInfo(callid){
                                  api('GET', 'subscribe-info/'+ callid, null, rsp => {

                                      var subscriberInfo = $('#subscriberInfo'+ callid);
                                      var subscriberBtn = $('#subscriberBtn'+ callid);
                                      var htm = '<h2 style=""text-align:left;padding-left:10px;"" id=\''+ ""conversationId"" + callid + '\'></h2>';
                                     
                                      htm += '<h2 style=""text-align:left;padding-left:10px;"" class=""card-title"">'+ ""Subscribe API URL: "" + '</h2>';
                                      if (rsp && rsp != 'Unable to build the subscribe api link') {
                                          var jsonResponse = JSON.parse(rsp);
                                          htm += '<div style=""text-align:left;padding:10px;word-wrap:break-word;"" class=""jumbotron"">'+ jsonResponse.url + '</div><br/>';
                                          htm += '<div class=""speaker"" id=\''+ ""speaker"" + callid + '\'></div><div class=""cc"" id=\''+ ""cc"" + callid + '\'>';
                                          htm += '<span class=""cc-text"" id=\''+ ""cc-text"" + callid + '\'>Begin speaking.</span></div>';
                                      }
                                      if (rsp && rsp != 'Unable to build the subscribe api link') {
                                        subscriberInfo.html(htm);

                                        var ws = wsSubscriberDict[callid];
                                        if ( typeof(ws) !== ""undefined"" && ws !== null ) {
                                            ws.close();
                                            delete wsSubscriberDict[callid];
                                        }
                                      }
                                       
                                      if($(subscriberBtn).text() == ""Show Subscriber""){  
                                        $(subscriberInfo).css('display', 'block');
                                        $(subscriberBtn).text('Hide Subscriber');
                                        console.log(""Subscribe URL:""+ jsonResponse.url);
                                        console.log(""Call Id: ""+ callid);
                                        SubscribeToWebSocket(jsonResponse.url, callid);
                                      }else{
                                        $(subscriberInfo).css('display', 'none');
                                        $(subscriberBtn).text('Show Subscriber');
                                      }
                                  });   
                              }
                              function updateCalls() {
                                  api('GET', 'calls', null, rsp => {
                                      var htm = '<table>';
                                      var calls = undefined;
                                      if (rsp) {
                                          calls = JSON.parse(rsp);
                                          htm += '<tr><th></th><th></th><th>Call Id</th><th>Scenario Id</th><th></th></tr>';
                                          for (var i = 0; i < calls.length; i++) {
                                              var call = calls[i];
                                              htm += '<tr>';
                                              htm += '<td><button class=""btn btn-danger"" onclick=""leave(\'' + call.legId + '\')"">Leave</button>';
                                              htm += '<td><button class=""btn btn-info"" id=\''+ ""subscriberBtn"" + call.legId + '\' onclick=""getSubscribeInfo(\'' + call.legId + '\')"">Show Subscriber</button>';
                                              htm += '<td>' + call.legId + '</td>';
                                              htm += '<td>' + call.scenarioId + '</td>';
                                              htm += '<td><a target=""_blank"" href=""' + call.logs + '"">Logs</a></td>';
                                              htm += '</tr>';
                                          }
                                      }
                                      htm += '</table>';
                                      if(calls != undefined)
                                      {
                                          for (var i = 0; i < calls.length; i++) {
                                            var call = calls[i];
                                            htm += '<br/><br/> <div id=\''+ ""subscriberInfo"" + call.legId + '\' ></div>';
                                          }
                                      }
                                      document.getElementById('calls').innerHTML = htm;
                                  });
                              }
                          </script>
                      </head>
                      <body onload='updateCalls()'>
                          <h1>Teams Bot</h1>
                          <input name='JoinURL' type='text' id='joinUrl' style=""width:500px"" />&nbsp;&nbsp;
                          <button onclick='join(document.getElementById(""joinUrl"").value)' class=""btn btn-success"">Join Meeting</button>
                          <hr />
                          <h1>List Calls</h1>
                          <button onclick='updateCalls()' class=""btn btn-secondary"">Update</button>
                          <br/><br/>
                          <div id='calls' />
                      </body>
                  </html>";
            return Content(html, "text/html", Encoding.UTF8); ;
        }
    }
}