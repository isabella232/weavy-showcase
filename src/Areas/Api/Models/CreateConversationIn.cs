using System.Collections.Generic;

namespace Weavy.Areas.Api.Models {

    /// <summary>
    /// Model for a creating a conversation.
    /// </summary>
    public class CreateConversationIn {

        /// <summary>
        /// The members of the conversation.
        /// </summary>
        public IEnumerable<int> Members { get; set; }
    }
}
