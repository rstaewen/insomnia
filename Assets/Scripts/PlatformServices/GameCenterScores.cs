using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class GameCenterScores : SocialScores
{
	#if UNITY_IPHONE

	void Start() {
		GameCenterManager.playerAuthenticated += OnAuthenticationSucceeded;
		GameCenterManager.reportScoreFinished += s => { Debug.Log("Score Submitted!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! " + s); };
		GameCenterManager.reportScoreFailed += s => { Debug.Log("Score Failed!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! " + s); };
	}

	public override void SubmitScore(int score) {
		if (GameCenterBinding.isPlayerAuthenticated()) {
			GameCenterBinding.reportScore(score, leaderBoardId);
		} else {
			GameCenterBinding.authenticateLocalPlayer();
			if (score > dirtyScore) {
				dirtyScore = score;
			}
		}
	}

	public override void ShowLeaderboard() {
		Debug.Log(GameCenterBinding.isPlayerAuthenticated());
		if (GameCenterBinding.isPlayerAuthenticated()) {
			GameCenterBinding.showLeaderboardWithTimeScopeAndLeaderboard(GameCenterLeaderboardTimeScope.Today, leaderBoardId);
		} else {
			GameCenterBinding.authenticateLocalPlayer();
			showLeaderBoard = true;
		}
	}
	
	#endif
}
