using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace LazerTanks
{
    class Shell
    {
        public Texture2D sprite;
        public float speed;
        public Vector2 position;
        public float angle;
        public Shell(Texture2D _sprite,float _speed, Vector2 _direction, float _angle)
        {
            sprite = _sprite;
            speed = _speed;
            position = _direction;
            angle = _angle;
        }
        public bool Hit(Tank _tank)
        {
            Rectangle tankSpriteRect = new Rectangle((int)_tank.position.X, (int)_tank.position.Y, _tank.sprite.Width, _tank.sprite.Height);
            Rectangle shellSpriteRect = new Rectangle((int)position.X, (int)position.Y, sprite.Width, sprite.Height);
            return tankSpriteRect.Intersects(shellSpriteRect);
        }
    }
}
