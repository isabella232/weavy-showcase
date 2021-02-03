using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;
using Weavy.Areas.Api.Models;
using Weavy.Core;
using Weavy.Core.Models;
using Weavy.Core.Services;
using Weavy.Web.Api.Controllers;
using Weavy.Web.Api.Models;

namespace Weavy.Areas.Api.Controllers {

    /// <summary>
    /// Api controller for manipulating Conversations.
    /// </summary>
    [RoutePrefix("api")]
    public class ConversationsController : WeavyApiController {

        /// <summary>
        /// Get the <see cref="Conversation"/> with the specified id.
        /// </summary>
        /// <param name="id">The conversation id.</param>
        /// <example>GET /api/conversations/527</example>
        /// <returns>The specified conversation.</returns>
        [HttpGet]
        [ResponseType(typeof(Conversation))]
        [Route("conversations/{id:int}")]
        public IHttpActionResult Get(int id) {
            // read conversation
            ConversationService.SetRead(id, DateTime.UtcNow);

            var conversation = ConversationService.Get(id);
            if (conversation == null) {
                ThrowResponseException(HttpStatusCode.NotFound, $"Conversation with id {id} not found.");
            }
            return Ok(conversation);
        }

        /// <summary>
        /// Get all <see cref="Conversation"/> for the current user.
        /// </summary>        
        /// <example>GET /api/conversations</example>
        /// <returns>The users conversations.</returns>
        [HttpGet]
        [ResponseType(typeof(IEnumerable<Conversation>))]
        [Route("conversations")]
        public IHttpActionResult List() {
            var conversations = ConversationService.Search(new ConversationQuery());
            return Ok(conversations);
        }

        /// <summary>
        /// Create a new or get the existing conversation between the current- and specified user.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ResponseType(typeof(Conversation))]
        [Route("conversations")]
        public IHttpActionResult Create(CreateConversationIn model) {
            string name = null;
            if (model.Members.Count() > 1) {
                name = string.Join(", ", model.Members.Select(u => UserService.Get(u).GetTitle()));
            }

            // create new room or one-on-one conversation or get the existing one
            return Ok(ConversationService.Insert(new Conversation() { Name = name }, model.Members));
        }

        /// <summary>
        /// Get the messages in the specified conversation.
        /// </summary>
        /// <param name="id">The conversation id.</param>
        /// <param name="opts">Query options for paging, sorting etc.</param>
        /// <returns>Returns a conversation.</returns>
        [HttpGet]
        [ResponseType(typeof(ScrollableList<Message>))]
        [Route("conversations/{id:int}/messages")]
        public IHttpActionResult GetMessages(int id, QueryOptions opts) {
            var conversation = ConversationService.Get(id);
            if (conversation == null) {
                ThrowResponseException(HttpStatusCode.NotFound, "Conversation with id " + id + " not found");
            }
            var messages = ConversationService.GetMessages(id, opts);
            messages.Reverse();
            return Ok(new ScrollableList<Message>(messages, Request.RequestUri));
        }

        /// <summary>
        /// Creates a new message in the specified conversation.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ResponseType(typeof(Message))]
        [Route("conversations/{id:int}/messages")]
        public IHttpActionResult InsertMessage(int id, InsertMessageIn model) {
            var conversation = ConversationService.Get(id);
            if (conversation == null) {
                ThrowResponseException(HttpStatusCode.NotFound, "Conversation with id " + id + " not found");
            }
            return Ok(MessageService.Insert(new Message { Text = model.Text, }, conversation));
        }

        /// <summary>
        /// Called by current user to indicate that they are typing in a conversation.
        /// </summary>
        /// <param name="id">Id of conversation.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("conversations/{id:int}/typing")]
        public IHttpActionResult StartTyping(int id) {
            var conversation = ConversationService.Get(id);
            // push typing event to other conversation members
            PushService.PushToUsers(PushService.EVENT_TYPING, new { Conversation = id, User = WeavyContext.Current.User, Name = WeavyContext.Current.User.Profile.Name ?? WeavyContext.Current.User.Username }, conversation.MemberIds.Where(x => x != WeavyContext.Current.User.Id));
            return Ok(conversation);
        }

        /// <summary>
        /// Called by current user to indicate that they are no longer typing.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("conversations/{id:int}/typing")]
        public IHttpActionResult StopTyping(int id) {
            var conversation = ConversationService.Get(id);
            // push typing event to other conversation members
            PushService.PushToUsers("typing-stopped.weavy", new { Conversation = id, User = WeavyContext.Current.User, Name = WeavyContext.Current.User.Profile.Name ?? WeavyContext.Current.User.Username }, conversation.MemberIds.Where(x => x != WeavyContext.Current.User.Id));
            return Ok(conversation);
        }

        /// <summary>
        /// Marks a conversation as read for the current user.
        /// </summary>
        /// <param name="id">Id of the conversation to mark as read.</param>
        /// <returns>The read conversation.</returns>
        [HttpPost]
        [Route("conversations/{id:int}/read")]
        public Conversation Read(int id) {
            ConversationService.SetRead(id, readAt: DateTime.UtcNow);
            return ConversationService.Get(id);
        }

        /// <summary>
        /// Get the number of unread conversations.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ResponseType(typeof(int))]
        [Route("conversations/unread")]
        public IHttpActionResult GetUnread() {
            return Ok(ConversationService.GetUnread().Count());
        }
    }
}
