namespace CloudBoard.Api.Constants
{
    /// <summary>
    /// Constants for board column validation and configuration
    /// </summary>
    public static class BoardConstants
    {
        /// <summary>
        /// Minimum number of columns a board must have
        /// </summary>
        public const int MinColumns = 1;

        /// <summary>
        /// Maximum number of columns a board can have
        /// </summary>
        public const int MaxColumns = 5;

        /// <summary>
        /// Maximum length for column names
        /// </summary>
        public const int MaxColumnNameLength = 50;

        /// <summary>
        /// Valid workflow categories for columns
        /// </summary>
        public static readonly string[] ValidCategories = { "Proposed", "InProgress", "Resolved" };

        /// <summary>
        /// Default columns created for new boards
        /// </summary>
        public static readonly (string Name, int Order, string Category)[] DefaultColumns = new[]
        {
            ("To Do", 0, "Proposed"),
            ("In Progress", 1, "InProgress"),
            ("Done", 2, "Resolved")
        };
    }
}
