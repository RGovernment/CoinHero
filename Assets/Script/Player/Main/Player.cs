using System.Collections.Generic;

public class Player : Character
{
    public Player(int id,string name, int maxHp,int nowHp ,List<Card> data) : base(id, name, maxHp, data)
    {
        HP = nowHp;
    }
}
