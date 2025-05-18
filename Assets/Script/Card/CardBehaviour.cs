using System;
using System.Drawing;

namespace CardGame.Card
{
    [Serializable]
    public abstract class CardBehaviour
    {
        public virtual void OnTurnStart() { }
        public virtual void OnTurnEnd() { }

        public virtual void OnDrawn() { }
        public virtual void OnPlaced() { }
        public virtual void OnDisposed() { }

        public virtual void OnAttack() { }
        public virtual void OnDefend() { }
        public virtual void OnDeath() { }

    }
}