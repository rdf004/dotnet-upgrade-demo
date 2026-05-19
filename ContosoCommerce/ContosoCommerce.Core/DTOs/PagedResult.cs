using System;
using System.Collections.Generic;

namespace ContosoCommerce.Core.DTOs
{
    /// <summary>
    /// Paginated result wrapper for list endpoints.
    /// </summary>
    [Serializable]
    public class PagedResult<T>
    {
        /// <summary>
        /// Items for the current page.
        /// </summary>
        public IList<T> Items { get; set; }

        /// <summary>
        /// Total number of records available.
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Current page number (1-based).
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// Number of items per page.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Total number of pages.
        /// </summary>
        public int TotalPages
        {
            get
            {
                if (PageSize <= 0) return 0;
                return (int)Math.Ceiling(
                    (double)TotalCount / PageSize);
            }
        }

        /// <summary>
        /// Whether a next page exists.
        /// </summary>
        public bool HasNextPage
        {
            get { return Page < TotalPages; }
        }

        /// <summary>
        /// Whether a previous page exists.
        /// </summary>
        public bool HasPreviousPage
        {
            get { return Page > 1; }
        }
    }
}
