using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class GameEngine : MonoBehaviour
{
    public static Sprite[] tileSpriteList;
    public Sprite[] tileSprites;
    public GameTile[][] blocks;
    public List<int> possible_blocks;
    public List<GameTile> game_tiles;
    public static Canvas static_canvas;
    public Canvas canvas;

    public static int Score = 0;
    public TMP_Text score_text;

    static private Vector3 right_vector = new Vector3(0.22f, 0, 0);
    static private Vector3 left_vector = new Vector3(-0.22f, 0, 0);
    static private Vector3 up_vector = new Vector3(0, 0.22f, 0);
    static private Vector3 down_vector = new Vector3(0, -0.22f, 0);

    private static int[] numbers = { 1, 1, 1, 1, 1, 1, 1, 1, 1, 2 };


    private int totalCoroutines = 0;
    private bool anyActionDone = false;
    // Start is called before the first frame update
    void Start()
    {
        tileSpriteList = tileSprites;
        static_canvas = canvas;
        blocks = new GameTile[4][];
        blocks[0] = new GameTile[4];
        blocks[1] = new GameTile[4];
        blocks[2] = new GameTile[4];
        blocks[3] = new GameTile[4];

        possible_blocks = new List<int>();
        for(int i =0; i<4; i++) {
            for(int j =0; j<4; j++)
            {
                possible_blocks.Add(i*4+j);
            }
        }

        RandomCreate();
        RandomCreate();

    }

    // Update is called once per frame
    void Update()
    {
        if (totalCoroutines==0)
        {
            if (IsLost()) LoseGame();
            if (anyActionDone)
            {
                anyActionDone = false;
                RandomCreate();
            }
            ResetMerges();
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                MoveRight();
            }
            else if(Input.GetKeyDown(KeyCode.LeftArrow)) 
            { 
                MoveLeft();
            }
            else if(Input.GetKeyDown(KeyCode.UpArrow))
            {
                MoveUp();
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
               MoveDown();
            }
        }
        

    }

    bool IsLost()
    {
        if (possible_blocks.Count != 0) return false;

        for(int x=1; x<3; x++)
        {
            for(int y=1; y<3; y++)
            {
                if (blocks[y][x].point == blocks[y - 1][x].point || blocks[y][x].point == blocks[y + 1][x].point || blocks[y][x].point == blocks[y][x + 1].point || blocks[y][x].point == blocks[y][x - 1].point) return false;
            }
        }

        for(int x=1; x<3; x++)
        {
            if (blocks[0][x].point == blocks[0][x - 1].point || blocks[0][x].point == blocks[0][x + 1].point || blocks[3][x].point == blocks[3][x - 1].point || blocks[3][x].point == blocks[3][x + 1].point) return false;
            if (blocks[x][0].point == blocks[x - 1][0].point || blocks[x][0].point == blocks[x + 1][0].point || blocks[x][3].point == blocks[x - 1][3].point || blocks[x][3].point == blocks[x + 1][3].point) return false;
        }
        return true;



    }
    void RandomCreate(bool only_two=true)
    {
        
        int point;
        if (only_two)
        {
            point = 1;
        }
        else
        {
            point = numbers[UnityEngine.Random.Range(0, 10)];
        }

        if(possible_blocks.Count == 0)
        {
            LoseGame();
            return;
        }
        int randomInt = UnityEngine.Random.Range(0, possible_blocks.Count);
        blocks[possible_blocks[randomInt] / 4][possible_blocks[randomInt] % 4] = new GameTile(possible_blocks[randomInt] % 4, possible_blocks[randomInt] / 4,point);
        possible_blocks.RemoveAt(randomInt);
    }

    void LoseGame()
    {
        if (Score > PlayerPrefs.GetInt("Score", 0))
        {
            PlayerPrefs.SetInt("Score", Score);
        }
        PlayerPrefs.SetInt("latestScore", Score);
        PlayerPrefs.Save();
        SceneManager.LoadScene("LostGame");
    }

    void MoveRight()
    {   
       for(int y = 0; y<4; y++)
        {
            int x = 2;
            while (x>-1)
            {
                if (blocks[y][x] != null)
                {
                    bool merge = false;
                    int total_move = 0;
                    Vector3 direction = new Vector3(0,0,0);
                    for(int x2 = x+1; x2 < 4;x2++)
                    {
                        if (blocks[y][x2] == null)
                        {
                            direction += right_vector;
                            total_move++;
                            continue;
                        }
                        else if (blocks[y][x2].point == blocks[y][x].point && !blocks[y][x].already_merged && !blocks[y][x2].already_merged)
                        {
                            direction += right_vector;
                            total_move++;
                            merge = true;
                        }
                        break;
                    }
                    if(total_move > 0)
                    {
                        anyActionDone = true;
                        GameTile temp = blocks[y][x + total_move];
                        blocks[y][x].x = x + total_move;
                        blocks[y][x+total_move]= blocks[y][x];
                        blocks[y][x] = temp;
                        if (merge)
                        {
                            ScoreUpdate(blocks[y][x + total_move].point * 2);
                            blocks[y][x+total_move].already_merged = true;
                        }
                        StartCoroutine(Animate(x+total_move,y,direction,merge,x,y));
                    }
                }
                x--;
            }
        }

       ResetMerges();

    }

    void MoveLeft()
    {
        for (int y = 0; y < 4; y++)
        {
            int x = 1;
            while (x < 4)
            {
                if (blocks[y][x] != null)
                {
                    bool merge = false;
                    int total_move = 0;
                    Vector3 direction = new Vector3(0, 0, 0);
                    for (int x2 = x - 1; x2 > -1; x2--)
                    {
                        if (blocks[y][x2] == null)
                        {
                            direction += left_vector;
                            total_move++;
                            continue;
                        }
                        else if (blocks[y][x2].point == blocks[y][x].point && !blocks[y][x].already_merged && !blocks[y][x2].already_merged)
                        {
                            direction += left_vector;
                            total_move++;
                            merge = true;
                        }
                        break;
                    }
                    if (total_move > 0)
                    {
                        anyActionDone = true;
                        GameTile temp = blocks[y][x - total_move];
                        blocks[y][x].x = x - total_move;
                        blocks[y][x - total_move] = blocks[y][x];
                        blocks[y][x] = temp;
                        if (merge)
                        {
                            ScoreUpdate(blocks[y][x - total_move].point * 2);
                            blocks[y][x-total_move].already_merged = true;
                        }
                        StartCoroutine(Animate(x - total_move, y, direction, merge, x, y));
                    }
                }
                x++;
            }
        }
        
        ResetMerges();
    }

    void MoveUp()
    {
        for (int x = 0; x < 4; x++)
        {
            int y = 2;
            while (y > -1)
            {
                if (blocks[y][x]!= null)
                {
                    bool merge = false;
                    int total_move = 0;
                    Vector3 direction = new Vector3(0, 0, 0);
                    for (int y2 = y + 1; y2 < 4; y2++)
                    {
                        if (blocks[y2][x] == null)
                        {
                            direction += up_vector;
                            total_move++;
                            continue;
                        }
                        else if (blocks[y2][x].point == blocks[y][x].point && !blocks[y2][x].already_merged && !blocks[y][x].already_merged)
                        {
                            direction += up_vector;
                            total_move++;
                            merge = true;
                        }
                        break;
                    }
                    if (total_move > 0)
                    {
                        anyActionDone = true;
                        GameTile temp = blocks[y+total_move][x];
                        blocks[y][x].y = y + total_move;
                        blocks[y+total_move][x] = blocks[y][x];
                        blocks[y][x] = temp;
                        if (merge)
                        {
                            ScoreUpdate(blocks[y+total_move][x].point * 2);

                            blocks[y + total_move][x].already_merged = true;
                        }
                        StartCoroutine(Animate(x, y+total_move, direction, merge, x, y));
                    }
                }
                y--;
            }
        }

        ResetMerges();
    }


    void MoveDown()
    {
        for (int x = 0; x < 4; x++)
        {
            int y = 1;
            while (y < 4)
            {
                if (blocks[y][x]!=null)
                {
                    bool merge = false;
                    int total_move = 0;
                    Vector3 direction = new Vector3(0, 0, 0);
                    for (int y2 = y - 1; y2 >-1; y2--)
                    {
                        if (blocks[y2][x] == null)
                        {
                            direction += down_vector;
                            total_move++;
                            continue;
                        }
                        else if (blocks[y2][x].point == blocks[y][x].point && !blocks[y2][x].already_merged && !blocks[y][x].already_merged)
                        {
                            direction += down_vector;
                            total_move++;
                            merge = true;
                        }
                        break;
                    }
                    if (total_move > 0)
                    {
                        anyActionDone = true;
                        GameTile temp = blocks[y - total_move][x];
                        blocks[y][x].y = y - total_move;
                        blocks[y - total_move][x] = blocks[y][x];
                        blocks[y][x] = temp;
                        if (merge)
                        {
                            ScoreUpdate(blocks[y - total_move][x].point * 2);
                            blocks[y - total_move][x].already_merged = true;
                        }
                        StartCoroutine(Animate(x, y - total_move, direction, merge, x, y));
                    }
                }
                y++;
            }
        }

        ResetMerges();
    }

    void ResetMerges()
    {
        for (int x = 0; x < 4; x++)
        {
            for (int y = 0; y < 4; y++)
            {
                if (blocks[y][x]!=null)
                {

                    blocks[y][x].already_merged = false;
                }
            }
        }
    }

    IEnumerator Animate(int first_x, int first_y,Vector3 direction,bool merge,int second_x,int second_y)
    {
        totalCoroutines++;
        float duration = 0.15f;
        float merge_duration = 0.10f;
        float elapsedTime = 0f;
        bool did_merge = false;
        Vector3 objectStartPosition = blocks[first_y][first_x].game_object.transform.position;
        Vector3 objectTargetPosition = objectStartPosition + direction;
        Vector3 textStartPosition = blocks[first_y][first_x].text_object.transform.position;
        Vector3 textTargetPosition = textStartPosition + direction;
        possible_blocks.Add(second_y * 4 + second_x);
        if (merge)
        {
            Destroy(blocks[second_y][second_x].text_object, merge_duration);
            Destroy(blocks[second_y][second_x].game_object, merge_duration);
            blocks[second_y][second_x] = null;
        }
        else
        {
            possible_blocks.Remove(first_y * 4 + first_x);
        }


        while (elapsedTime < duration) {
            blocks[first_y][first_x].game_object.transform.position = Vector3.Lerp(objectStartPosition, objectTargetPosition, elapsedTime / duration);
            blocks[first_y][first_x].text_object.transform.position = Vector3.Lerp(textStartPosition, textTargetPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            if(merge && !did_merge && elapsedTime >= merge_duration)
            {
                did_merge = true;
                blocks[first_y][first_x].sprite_renderer.sprite = tileSpriteList[blocks[first_y][first_x].log_point++];
                blocks[first_y][first_x].point *= 2;
                blocks[first_y][first_x].text.text = blocks[first_y][first_x].point.ToString();
            }
            yield return null;
        }
        if (merge && !did_merge && elapsedTime >= merge_duration)
        {
            did_merge = true;
            blocks[first_y][first_x].sprite_renderer.sprite = tileSpriteList[blocks[first_y][first_x].log_point++];
            blocks[first_y][first_x].point *= 2;
            blocks[first_y][first_x].text.text = blocks[first_y][first_x].point.ToString();

        }
        blocks[first_y][first_x].game_object.transform.position = objectTargetPosition;
        blocks[first_y][first_x].text_object.transform.position = textTargetPosition;
        totalCoroutines--;
    }

    void ScoreUpdate(int point)
    {
        Score += point;
        score_text.text = "Score: "+Score.ToString();
    }

}


public class GameTile
{
    public int x;
    public int y;
    public int log_point;
    public int point;
    public GameObject game_object;
    public GameObject text_object;
    public SpriteRenderer sprite_renderer;
    public static Canvas canvas = GameEngine.static_canvas;
    public RectTransform rectTransform;
    public TMP_Text text;
    public bool already_merged = false;
    public GameTile(int x,int y,int point = 1)
    {
        this.x = x;
        this.y = y;
        this.log_point = point;
        this.point = (int)math.pow(2, point);
        
        game_object = new GameObject();
        text_object = new GameObject();
        
        sprite_renderer = game_object.AddComponent<SpriteRenderer>();
        sprite_renderer.sprite = GameEngine.tileSpriteList[log_point-1];
        game_object.transform.position = new Vector3(-0.33f+0.22f*x,0.03f+y*0.22f,0);
        text_object.transform.SetParent(canvas.transform,false);
        text=text_object.AddComponent<TextMeshProUGUI>();
        text.text = this.point.ToString();
        rectTransform = text_object.GetComponent<RectTransform>();
        rectTransform.position = new Vector3(-0.33f + 0.22f * x, 0.13f + 0.22f * y, 0);
        rectTransform.sizeDelta = new Vector2(0.2f, 0.2f);
        text.fontSizeMin = 0;
        text.enableAutoSizing = true;
        text.alignment = TextAlignmentOptions.Center;
    }
}