using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public interface ISocialScores
{
	void SubmitScore(string leaderBoardID, int score);
	void ShowLeaderboard();
}
