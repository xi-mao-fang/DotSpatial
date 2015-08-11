// ********************************************************************************************************
// Product Name: MapWindow.dll Alpha
// Description:  The basic module for MapWindow version 6.0
// ********************************************************************************************************
// The contents of this file are subject to the Mozilla Public License Version 1.1 (the "License");
// you may not use this file except in compliance with the License. You may obtain a copy of the License at
// http://www.mozilla.org/MPL/  Alternately, you can access an earlier version of this content from
// the Net Topology Suite, which is protected by the GNU Lesser Public License
// http://dotspatial.codeplex.com/license and the sourcecode for the Net Topology Suite
// can be obtained here: http://sourceforge.net/projects/nts.
//
// Software distributed under the License is distributed on an "AS IS" basis, WITHOUT WARRANTY OF
// ANY KIND, either expressed or implied. See the License for the specific language governing rights and
// limitations under the License.
//
// The Original Code is from the Net Topology Suite
//
// The Initial Developer to integrate this code into MapWindow 6.0 is Ted Dunsford.
//
// Contributor(s): (Open source contributors should list themselves and their modifications here).
//
// ********************************************************************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using DotSpatial.Topology.Geometries;
using DotSpatial.Topology.Utilities;
using Wintellect.PowerCollections;

namespace DotSpatial.Topology.Noding
{
    /// <summary>
    /// A list of the <see cref="SegmentNode" />s present along a noded <see cref="ISegmentString"/>.
    /// </summary>
    public class SegmentNodeList : IEnumerable<object>
    {
        #region Fields

        private readonly NodedSegmentString _edge;  // the parent edge
        private readonly IDictionary<SegmentNode, object> _nodeMap = new OrderedDictionary<SegmentNode, object>();

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SegmentNodeList"/> class.
        /// </summary>
        /// <param name="edge">The edge.</param>
        public SegmentNodeList(NodedSegmentString edge)
        {
            _edge = edge;
        }

        #endregion

        #region Properties

        /// <summary>
        ///
        /// </summary>
        /// <value></value>
        public NodedSegmentString Edge 
        {
            get { return _edge; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Adds an intersection into the list, if it isn't already there.
        /// The input segmentIndex and dist are expected to be normalized.
        /// </summary>
        /// <param name="intPt"></param>
        /// <param name="segmentIndex"></param>
        /// <returns>The SegmentIntersection found or added.</returns>
        public SegmentNode Add(Coordinate intPt, int segmentIndex)
        {
            var eiNew = new SegmentNode(_edge, intPt, segmentIndex, _edge.GetSegmentOctant(segmentIndex));
            object eiObj;
            if (_nodeMap.TryGetValue(eiNew, out eiObj))
            {
                var ei = (SegmentNode)eiObj;
                // debugging sanity check
                Assert.IsTrue(ei.Coordinate.Equals2D(intPt), "Found equal nodes with different coordinates");               
                return ei;
            }
            // node does not exist, so create it
            _nodeMap.Add(eiNew, eiNew);
            return eiNew;
        }

        /// <summary>
        /// Creates new edges for all the edges that the intersections in this
        /// list split the parent edge into.
        /// Adds the edges to the provided argument list
        /// (this is so a single list can be used to accumulate all split edges
        /// for a set of <see cref="ISegmentString" />s).
        /// </summary>
        /// <param name="edgeList"></param>
        public void AddSplitEdges(IList<ISegmentString> edgeList)
        {
            // ensure that the list has entries for the first and last point of the edge
            AddEndPoints();
            AddCollapsedNodes();

            // there should always be at least two entries in the list, since the endpoints are nodes
            var ie = GetEnumerator();
	        ie.MoveNext();            
            var eiPrev = (SegmentNode) ie.Current;
            while (ie.MoveNext())
            {
                var ei = (SegmentNode)ie.Current;
                var newEdge = CreateSplitEdge(eiPrev, ei);
                edgeList.Add(newEdge);
                eiPrev = ei;
            }
        }

        /// <summary>
        /// Returns an iterator of SegmentNodes.
        /// </summary>
        /// <returns>An iterator of SegmentNodes.</returns>
        public IEnumerator<object>GetEnumerator() 
        { 
            return _nodeMap.Values.GetEnumerator(); 
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="outstream"></param>
        public void Write(StreamWriter outstream)
        {
            outstream.Write("Intersections:");
            foreach (object obj in this)
            {
                SegmentNode ei = (SegmentNode)obj;
                ei.Write(outstream);
            }
        }

        /// <summary>
        /// Adds nodes for any collapsed edge pairs.
        /// Collapsed edge pairs can be caused by inserted nodes, or they can be
        /// pre-existing in the edge vertex list.
        /// In order to provide the correct fully noded semantics,
        /// the vertex at the base of a collapsed pair must also be added as a node.
        /// </summary>
        private void AddCollapsedNodes()
        {
            IList<int> collapsedVertexIndexes = new List<int>();

            FindCollapsesFromInsertedNodes(collapsedVertexIndexes);
            FindCollapsesFromExistingVertices(collapsedVertexIndexes);

            // node the collapses
            foreach(var vertexIndex in collapsedVertexIndexes)
                Add(_edge.GetCoordinate(vertexIndex), vertexIndex);
        }

        /// <summary>
        /// Adds nodes for the first and last points of the edge.
        /// </summary>
        private void AddEndPoints()
        {
            int maxSegIndex = _edge.Count - 1;
            Add(_edge.GetCoordinate(0), 0);
            Add(_edge.GetCoordinate(maxSegIndex), maxSegIndex);
        }

        /// <summary>
        ///  Create a new "split edge" with the section of points between
        /// (and including) the two intersections.
        /// The label for the new edge is the same as the label for the parent edge.
        /// </summary>
        /// <param name="ei0"></param>
        /// <param name="ei1"></param>
        /// <returns></returns>
        ISegmentString CreateSplitEdge(SegmentNode ei0, SegmentNode ei1)
        {
            var npts = ei1.SegmentIndex - ei0.SegmentIndex + 2;

            var lastSegStartPt = _edge.GetCoordinate(ei1.SegmentIndex);
            // if the last intersection point is not equal to the its segment start pt, add it to the points list as well.
            // (This check is needed because the distance metric is not totally reliable!)
            // The check for point equality is 2D only - Z values are ignored
            var useIntPt1 = ei1.IsInterior || !ei1.Coordinate.Equals2D(lastSegStartPt);
            if(!useIntPt1)
                npts--;

            var pts = new Coordinate[npts];
            var ipt = 0;
            pts[ipt++] = new Coordinate(ei0.Coordinate);
            for (var i = ei0.SegmentIndex + 1; i <= ei1.SegmentIndex; i++)
                pts[ipt++] = _edge.GetCoordinate(i);            
            if (useIntPt1) 
                pts[ipt] = ei1.Coordinate;

            return new NodedSegmentString(pts, _edge.Context);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="ei0"></param>
        /// <param name="ei1"></param>
        /// <param name="collapsedVertexIndex"></param>
        /// <returns></returns>
        private static bool FindCollapseIndex(SegmentNode ei0, SegmentNode ei1, int[] collapsedVertexIndex)
        {
            // only looking for equal nodes
            if (!ei0.Coordinate.Equals2D(ei1.Coordinate)) 
                return false;
            var numVerticesBetween = ei1.SegmentIndex - ei0.SegmentIndex;
            if (!ei1.IsInterior)
                numVerticesBetween--;
            // if there is a single vertex between the two equal nodes, this is a collapse
            if (numVerticesBetween == 1)
            {
                collapsedVertexIndex[0] = ei0.SegmentIndex + 1;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Adds nodes for any collapsed edge pairs
        /// which are pre-existing in the vertex list.
        /// </summary>
        /// <param name="collapsedVertexIndexes"></param>
        private void FindCollapsesFromExistingVertices(IList<int> collapsedVertexIndexes)
        {
            for (var i = 0; i < _edge.Count - 2; i++)
            {
                var p0 = _edge.GetCoordinate(i);
                //var p1 = _edge.GetCoordinate(i + 1);
                var p2 = _edge.GetCoordinate(i + 2);
                if (p0.Equals2D(p2))    // add base of collapse as node
                    collapsedVertexIndexes.Add(i + 1);                
            }
        }

        /// <summary>
        /// Adds nodes for any collapsed edge pairs caused by inserted nodes
        /// Collapsed edge pairs occur when the same coordinate is inserted as a node
        /// both before and after an existing edge vertex.
        /// To provide the correct fully noded semantics,
        /// the vertex must be added as a node as well.
        /// </summary>
        /// <param name="collapsedVertexIndexes"></param>
        private void FindCollapsesFromInsertedNodes(IList<int> collapsedVertexIndexes)
        {
            var collapsedVertexIndex = new int[1];
            
	        var ie = GetEnumerator();
	        ie.MoveNext();

            // there should always be at least two entries in the list, since the endpoints are nodes
            var eiPrev = (SegmentNode) ie.Current;
            while (ie.MoveNext())
            {
                var ei = (SegmentNode) ie.Current;
                var isCollapsed = FindCollapseIndex(eiPrev, ei, collapsedVertexIndex);
                if (isCollapsed)
                    collapsedVertexIndexes.Add(collapsedVertexIndex[0]);
                eiPrev = ei;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    class NodeVertexIterator : IEnumerator<object>
    {
        #region Fields

        private readonly IEnumerator<object> _nodeIt;
        private SegmentNode _currNode;
        private int _currSegIndex;
        private ISegmentString _edge;
        private SegmentNode _nextNode;
        private SegmentNodeList _nodeList;

        #endregion

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nodeList"></param>
        NodeVertexIterator(SegmentNodeList nodeList)
        {
            _nodeList = nodeList;
            _edge = nodeList.Edge;
            _nodeIt = nodeList.GetEnumerator();            
        }

        #endregion

        #region Properties

        /// <summary>
        /// 
        /// </summary>
        public object Current
        {
            get  { return _currNode; }
        }

        #endregion

        #region Methods

        public void Dispose()
        {
            //throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool MoveNext()
        {
            if (_currNode == null)
            {
                _currNode = _nextNode;
                _currSegIndex = _currNode.SegmentIndex;
                ReadNextNode();
                return true;
            }

            // check for trying to read too far
            if (_nextNode == null) 
                return false;

            if (_nextNode.SegmentIndex == _currNode.SegmentIndex)
            {
                _currNode = _nextNode;
                _currSegIndex = _currNode.SegmentIndex;
                ReadNextNode();
                return true;
            }

            if (_nextNode.SegmentIndex > _currNode.SegmentIndex)
            {

            }
            return false;
        }

        /// <summary>
        /// Not implemented.
        /// </summary>
        /// <exception cref="NotSupportedException">This method is not implemented.</exception>
        [Obsolete("Not implemented!")]
        public void Remove()
        {
            throw new NotSupportedException(GetType().Name);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Reset()
        {
            _nodeIt.Reset();            
        }

        /// <summary>
        /// 
        /// </summary>
        private void ReadNextNode()
        {
            if (_nodeIt.MoveNext())
                 _nextNode = (SegmentNode) _nodeIt.Current;
            else _nextNode = null;
        }

        #endregion
    }
}