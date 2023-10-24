using System;

namespace SA3D.Archival
{
	/// <summary>
	/// Represents errors that occur when the wrong Archive type was used.
	/// </summary>
	public class InvalidArchiveException : Exception
	{
		/// <inheritdoc/>
		public InvalidArchiveException(string msg) : base(msg) { }
	}
}
