﻿using System;

namespace GitHub.ViewModels
{
    /// <summary>
    /// Represents a file or directory node in a pull request changes tree.
    /// </summary>
    public interface IPullRequestChangeNode
    {
        /// <summary>
        /// Gets the path to the file or directory, relative to the root of the repository.
        /// </summary>
        string Path { get; }
    }
}