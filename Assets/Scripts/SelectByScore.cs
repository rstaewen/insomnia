using UnityEngine;
using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

public class SelectByScore : MonoBehaviour {

	[System.Serializable] public class SelectionNode : IComparable<SelectionNode>
	{
		public GameObject selection;
		public int minScore;

		int IComparable<SelectionNode>.CompareTo(SelectionNode other)
		{
			if (other.minScore > minScore)
				return -1;
			else if (other.minScore == minScore)
				return 0;
			else
				return 1;
		}
	}
	public Score scoreTracker;
	public List<SelectionNode> scoreSelections;

	void Awake()
	{
		scoreTracker.onScoreIncreased += onScoreUp;
		onScoreUp();
	}

	void onScoreUp()
	{
		IEnumerable<SelectionNode> minNodes = scoreSelections.Where(ss => ss.minScore <= scoreTracker.CurrentUnadjusted);
		if(minNodes.Count() > 0)
		{
			foreach(SelectionNode sn in scoreSelections)
				sn.selection.SetActive(false);
			minNodes.Max().selection.SetActive(true);
		}
	}
}
