using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public abstract class SocialScores : MonoBehaviour, ISocialScores
{
	[SerializeField] protected string leaderBoardId;

	protected int? dirtyScore;
	protected bool showLeaderBoard;

	protected void OnAuthenticationSucceeded() {
		DebugUtility.AddLine("authentication succeeded!");
		if (dirtyScore != null) {
			SubmitScore(leaderBoardId, dirtyScore.Value);
			dirtyScore = null;
		 } else if (showLeaderBoard) {
			ShowLeaderboard();
			showLeaderBoard = false;
		}
	}

	protected void OnAuthenticationFailed() {
		DebugUtility.AddLine("authentication failed!");
//		if (dirtyScore != null) {
//			SubmitScore(leaderBoardId, dirtyScore.Value);
//			dirtyScore = null;
//		} else if (showLeaderBoard) {
//			ShowLeaderboard();
//			showLeaderBoard = false;
//		}
	}

	public virtual void SubmitScore(string leaderBoardID, int score){}

	public virtual void ShowLeaderboard() {}

}
