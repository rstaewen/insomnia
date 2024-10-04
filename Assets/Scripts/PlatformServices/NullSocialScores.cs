using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class NullSocialScores : SocialScores
{
	public override void SubmitScore(string leaderBoardID, int score) {}
	public override void ShowLeaderboard() {}
}
