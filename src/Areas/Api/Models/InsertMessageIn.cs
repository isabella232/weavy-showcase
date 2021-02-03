using System.Collections.Generic;

namespace Weavy.Areas.Api.Models {

    /// <summary>
    /// Model for inserting a message into a conversation.
    /// </summary>
    public class InsertMessageIn {

        /// <summary>
        /// The message text.
        /// </summary>
        public string Text { get; set; }
    }
}
