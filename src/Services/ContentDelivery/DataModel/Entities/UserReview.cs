// File: UserReview.cs
// Copyright (c) 2018-2019 Maksym Shnurenok
// License: MIT
using System.Collections.Generic;

namespace Marketplace.Services.ContentDelivery.DataModel.Entities
{
    /// <summary>
    /// User review class
    /// </summary>
    /// <seealso cref="UserComment" />
    internal class UserReview
    {
        /// <summary>
        /// Gets or sets the score.
        /// </summary>
        /// <value>
        /// The score.
        /// </value>
        public int Score { get; set; }

        /// <summary>
        /// Gets or sets the pros.
        /// </summary>
        /// <value>
        /// The pros.
        /// </value>
        public string Pros { get; set; }

        /// <summary>
        /// Gets or sets the cons.
        /// </summary>
        /// <value>
        /// The cons.
        /// </value>
        public string Cons { get; set; }

        /// <summary>
        /// Gets or sets the helpful user scores.
        /// </summary>
        /// <value>
        /// The helpful user scores.
        /// </value>
        public ICollection<long> HelpfulUserIdScores { get; set; }

        /// <summary>
        /// Gets or sets the not helpful user scores.
        /// </summary>
        /// <value>
        /// The not helpful user scores.
        /// </value>
        public ICollection<long> NotHelpfulUserIdScores { get; set; }
    }
}
