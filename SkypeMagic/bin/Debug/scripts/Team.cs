using SkypeMagic;
using System;

class TM : Script
{
    public void Team()
    {
        Skype.SendMessageToConv("Pridruži se timu Cuki (!cuki) ili timu Kiki (!kiki)");
        Fork(100 * 60, Tuple.Create(0, 0), (msg, state) => {
            if (msg.Text == "!cuki")
            {
                var newState = Tuple.Create(state.Item1 + 1, state.Item2);
                Skype.SendMessageToConv(newState.Item1 + " članova Team Cuki");
                return newState;
            }
            else if (msg.Text == "!kiki")
            {
                var newState = Tuple.Create(state.Item1, state.Item2 + 1);
                Skype.SendMessageToConv(newState.Item1 + " članova Team Kiki");
                return newState;
            }
            else if (msg.Text == "!stanje")
            {
                Skype.SendMessageToConv("Cuki - " + state.Item1 + ", Kiki - " + state.Item2);
            }
            return state;
        });
    }
}