// ********************************************************************************************************
// Product Name: DotSpatial.Topology.dll
// Description:  The basic topology module for the new dotSpatial libraries
// ********************************************************************************************************
// The contents of this file are subject to the Lesser GNU Public License (LGPL)
// you may not use this file except in compliance with the License. You may obtain a copy of the License at
// http://dotspatial.codeplex.com/license  Alternately, you can access an earlier version of this content from
// the Net Topology Suite, which is also protected by the GNU Lesser Public License and the sourcecode
// for the Net Topology Suite can be obtained here: http://sourceforge.net/projects/nts.
//
// Software distributed under the License is distributed on an "AS IS" basis, WITHOUT WARRANTY OF
// ANY KIND, either expressed or implied. See the License for the specific language governing rights and
// limitations under the License.
//
// The Original Code is from the Net Topology Suite, which is a C# port of the Java Topology Suite.
//
// The Initial Developer to integrate this code into MapWindow 6.0 is Ted Dunsford.
//
// Contributor(s): (Open source contributors should list themselves and their modifications here).
// |         Name         |    Date    |                              Comment
// |----------------------|------------|------------------------------------------------------------
// |                      |            |
// ********************************************************************************************************

namespace DotSpatial.Topology.Noding
{
    /// <summary>
    /// Processes possible intersections detected by a <see cref="INoder"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <see cref="ISegmentIntersector" /> is passed to a <see cref="INoder" />.
    /// </para>
    /// The <see cref="ISegmentIntersector.ProcessIntersections(ISegmentString, int, ISegmentString, int)"/> 
    /// method is called whenever the <see cref="INoder" />
    ///  detects that two <see cref="ISegmentString" />s might intersect.
    /// <para>
    /// This class may be used either to find all intersections, or
    /// to detect the presence of an intersection.  In the latter case,
    /// Noders may choose to short-circuit their computation by calling the
    /// <see cref="IsDone"/> property.
    /// </para>
    /// <para>
    /// </para>
    /// This class is an example of the <i>Strategy</i> pattern.
    /// <para>
    /// This class may be used either to find all intersections, or
    /// to detect the presence of an intersection.  In the latter case,
    /// Noders may choose to short-circuit their computation by calling the
    /// <see cref="IsDone"/> property.
    /// </para>
    /// </remarks>
    public interface ISegmentIntersector
    {
        #region Properties

        ///<summary>
        /// Reports whether the client of this class needs to continue testing
        /// all intersections in an arrangement.
        ///</summary>
        ///<returns>if there is no need to continue testing segments</returns>
        bool IsDone { get; }

        #endregion

        #region Methods

        /// <summary>
        /// This method is called by clients
        /// of the <see cref="ISegmentIntersector" /> interface to process
        /// intersections for two segments of the <see cref="ISegmentString" />s being intersected.
        /// </summary>
        /// <param name="e0"></param>
        /// <param name="segIndex0"></param>
        /// <param name="e1"></param>
        /// <param name="segIndex1"></param>
        void ProcessIntersections(ISegmentString e0, int segIndex0, ISegmentString e1, int segIndex1);

        #endregion
    }
}