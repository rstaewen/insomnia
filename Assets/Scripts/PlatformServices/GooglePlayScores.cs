using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class GooglePlayScores : SocialScores
{
	#if UNITY_ANDROID

	private int authenticationCount;

	void Start() {
		GPGManager.authenticationSucceededEvent += GooglePlayOnAuthentication;
		GPGManager.authenticationFailedEvent += GooglePlayOnAuthFailed;
		GPGManager.submitScoreSucceededEvent += (s, o) => { Debug.Log("Score Submitted!!!!!!!! " + s); };
		GPGManager.submitScoreFailedEvent += (s, o) => { Debug.Log("Score Failed!!!!!!! " + s); };
	}

	void GooglePlayOnAuthentication(string userId) {
		OnAuthenticationSucceeded();
	}

	void GooglePlayOnAuthFailed(string userId) {
		OnAuthenticationFailed();
	}

	public override void SubmitScore(string leaderBoardID, int score) {
		this.leaderBoardId = leaderBoardID;
		if (PlayGameServices.isSignedIn()) {
			DebugUtility.AddLine("submitting scores: signed in, submit!", "scoresubmit");
			PlayGameServices.submitScore(leaderBoardId, score);
		} else {
			if (authenticationCount < 1) {
				DebugUtility.AddLine("submitting scores: logging in...", "scoresubmit");
				PlayGameServices.authenticate();
				authenticationCount++;
			}
			if (score > dirtyScore) {
				dirtyScore = score;
			}
		}
	}

	public override void ShowLeaderboard() {
		if (PlayGameServices.isSignedIn()) {
			DebugUtility.AddLine("showleaderboard: signed in, opening!", "leaderboard");
			PlayGameServices.showLeaderboard(leaderBoardId, GPGLeaderboardTimeScope.Today);
		} else {
			DebugUtility.AddLine("showleaderboard: authenticate reqd.", "leaderboard");
			PlayGameServices.authenticate();
			showLeaderBoard = true;
		}

	}

	#endif
}
