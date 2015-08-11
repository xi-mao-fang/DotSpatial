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

using System;
using System.Collections.Generic;
using DotSpatial.Topology.Algorithm;
using DotSpatial.Topology.Geometries;

namespace DotSpatial.Topology.Noding
{
    /// <summary>
    /// Finds <b>interior</b> intersections 
    /// between line segments in <see cref="NodedSegmentString"/>s,
    /// and adds them as nodes
    /// using <see cref="NodedSegmentString.AddIntersection(LineIntersector,int,int,int)"/>.
    /// This class is used primarily for Snap-Rounding.  
    /// For general-purpose noding, use <see cref="IntersectionAdder"/>.
    /// </summary>
    /// <remarks>
    /// This class is obsolete. 
    /// Use <see cref="InteriorIntersectionFinderAdder"/> instead.
    /// </remarks>
    /// <seealso cref="IntersectionAdder"/>
    /// <seealso cref="InteriorIntersectionFinderAdder"/>
    [Obsolete("see InteriorIntersectionFinderAdder")]
    public class IntersectionFinderAdder : ISegmentIntersector
    {
        #region Fields

        private readonly IList<Coordinate> _interiorIntersections;
        private readonly LineIntersector _li;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates an intersection finder which finds all proper intersections.
        /// </summary>
        /// <param name="li">The <see cref="LineIntersector" /> to use.</param>
        public IntersectionFinderAdder(LineIntersector li)
        {
            _li = li;
            _interiorIntersections = new List<Coordinate>();
        }

        #endregion

        #region Properties

        /// <summary>
        /// 
        /// </summary>
        public IList<Coordinate> InteriorIntersections
        {
            get { return _interiorIntersections; }
        }

        ///<summary>
        /// Always process all intersections
        ///</summary>
        public bool IsDone
        {
            get { return false; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// This method is called by clients
        /// of the <see cref="ISegmentIntersector" /> class to process
        /// intersections for two segments of the <see cref="ISegmentString" />s being intersected.<br/>
        /// Note that some clients (such as <c>MonotoneChain</c>s) may optimize away
        /// this call for segment pairs which they have determined do not intersect
        /// (e.g. by an disjoint envelope test).
        /// </summary>
        /// <param name="e0"></param>
        /// <param name="segIndex0"></param>
        /// <param name="e1"></param>
        /// <param name="segIndex1"></param>
        public void ProcessIntersections(ISegmentString e0, int segIndex0, ISegmentString e1, int segIndex1)
        {
            // don't bother intersecting a segment with itself
            if (e0 == e1 && segIndex0 == segIndex1)
                return;

            var coordinates0 = e0.Coordinates;
            Coordinate p00 = coordinates0[segIndex0];
            Coordinate p01 = coordinates0[segIndex0 + 1];
            var coordinates1 = e1.Coordinates;
            Coordinate p10 = coordinates1[segIndex1];
            Coordinate p11 = coordinates1[segIndex1 + 1];
            _li.ComputeIntersection(p00, p01, p10, p11);

            if (!_li.HasIntersection) return;
            if (!_li.IsInteriorIntersection()) return;
            for (int intIndex = 0; intIndex < _li.IntersectionNum; intIndex++)
                _interiorIntersections.Add(_li.GetIntersection(intIndex));

            NodedSegmentString nss0 = (NodedSegmentString)e0;
            nss0.AddIntersections(_li, segIndex0, 0);
            NodedSegmentString nss1 = (NodedSegmentString)e1;
            nss1.AddIntersections(_li, segIndex1, 1);
        }

        #endregion
    }
}