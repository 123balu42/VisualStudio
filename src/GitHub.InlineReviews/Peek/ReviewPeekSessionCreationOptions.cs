﻿using System;
using System.Collections.Generic;
using GitHub.Models;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace GitHub.InlineReviews.Peek
{
    class ReviewPeekSessionCreationOptions : PeekSessionCreationOptions
    {
        public ReviewPeekSessionCreationOptions(
            ITextView textView,
            ITrackingPoint triggerPoint,
            IList<IPullRequestReviewCommentModel> comments)
            : base(textView, ReviewPeekRelationship.Instance.Name, triggerPoint)
        {
            Comments = comments;
        }

        public IList<IPullRequestReviewCommentModel> Comments { get; }
    }
}
