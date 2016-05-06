using UnityEngine;
using System.Collections;

public class GameConsts
{
    public const int MAX_GENERATIONS = 1000;
    public const int ATTACK_PLAYER_COUNT = 20;
    public const int DEFENSE_PLAYER_COUNT = 32;
    public const int GOALLY_PLAYER_COUNT = 20;
    public const float BALL_HIT_STRENGHT_SCALE = 20f;

    public const float GAME_FIELD_LEFT = -3.05f;
    public const float GAME_FIELD_RIGHT = 3.05f;
    public const float GAME_FIELD_UP = 2.88f;
    public const float GAME_FIELD_DOWN = -2.88f;
    public const float GOAL_UP = 0.85f;
    public const float GOAL_DOWN = -0.85f;

    public const string DEFENSE_PLAYER = "DEFENSE";
    public const string ATTACK_PLAYER = "ATTACK";
    public const string GOALLY_PLAYER = "GOALLY";
}
