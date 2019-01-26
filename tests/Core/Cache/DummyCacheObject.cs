// File: DummyCacheObject.cs
// Copyright (c) 2018-2019 Maksym Shnurenok
// License: MIT
using System;
using System.Collections.Generic;

namespace Marketplace.Core.Tests.Cache
{
    /// <summary>
    /// The dummy cache object
    /// </summary>
    [Serializable]
    public class DummyCacheObject: IEquatable<DummyCacheObject>
    {
        #region Properties

        /// <summary>
        /// Gets or sets the dummy int.
        /// </summary>
        /// <value>
        /// The dummy int.
        /// </value>
        public int DummyInt { get; set; } = 123;

        /// <summary>
        /// Gets or sets the dummy string.
        /// </summary>
        /// <value>
        /// The dummy string.
        /// </value>
        public string DummyString { get; set; } = "MyString";

        /// <summary>
        /// Gets the dummy children.
        /// </summary>
        /// <value>
        /// The dummy children.
        /// </value>
        public List<DummyCacheObject> DummyChildren { get; } =
            new List<DummyCacheObject>();

        #endregion

        #region Methods

        /// <summary>
        /// Gets the test object.
        /// </summary>
        /// <returns>Typical test object with predefined values.</returns>
        public static DummyCacheObject GetTestObject()
        {
            var dummyCacheObject = new DummyCacheObject();
            dummyCacheObject.DummyChildren.Add(new DummyCacheObject() { DummyInt = 456, DummyString = "MyChildString1" });
            dummyCacheObject.DummyChildren.Add(new DummyCacheObject() { DummyInt = 789, DummyString = "MyChildString2" });
            return dummyCacheObject;
        }

        /// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
        public bool Equals(DummyCacheObject other)
        {
            bool initialFactor = other != null && this.DummyInt == other.DummyInt && this.DummyString == other.DummyString
                                 && this.DummyChildren.Count == other.DummyChildren.Count;

            if (!initialFactor)
            {
                return false;
            }

            for (int x = 0; x < this.DummyChildren.Count; x++)
            {
                if (!this.DummyChildren[x].Equals(other.DummyChildren[x]))
                {
                    return false;
                }
            }

            return true;
        }

        #endregion
    }
}
