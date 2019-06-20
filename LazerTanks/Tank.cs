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
    class Tank
    {
        public int HP;
        public bool alive = true;
        public float speed;
        public float turn_speed;
        public float rotationAngle;
        public float rechair;
        public float timeAfterShoot = 0;
        public float timeRechairBaraban = 100f;
        public int shells_on_baraban = 5;
        public Texture2D sprite;
        public Texture2D shell;
        public List<Shell> delete_shells = new List<Shell>();
        public float shell_speed;
        public ICollection<Shell> shells;
        public Vector2 origin;//центральна точна спрайта
        public Vector2 position;
        
        GraphicsDeviceManager graphics;
        public Tank(Texture2D _sprite, Texture2D _shell, Vector2 _position,float _rechair,float _shell_speed, float _speed, float _rotationAngle, int _HP, float _turn_speed, GraphicsDeviceManager _graphics)
        {
            position = _position;
            sprite = _sprite;
            origin.X = sprite.Width / 2 ;
            origin.Y = sprite.Height / 2 ;
            speed = _speed;
            rotationAngle = _rotationAngle;
            HP = _HP;
            turn_speed = _turn_speed;
            graphics = _graphics;
            shells = new List<Shell>();
            shell = _shell;
            rechair = _rechair;
            shell_speed = _shell_speed;
        }
        public void Move(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Up))
            {
                Vector2 temp = position;
                temp += new Vector2((float)(Math.Sin(rotationAngle)) * speed, (float)(-Math.Cos(rotationAngle)) * speed);
                if (temp.Y < graphics.PreferredBackBufferHeight - origin.Y && temp.Y > origin.Y && temp.X < graphics.PreferredBackBufferWidth - origin.X && temp.X > origin.X)
                    this.position += new Vector2((float)(Math.Sin(rotationAngle)) * speed, (float)(-Math.Cos(rotationAngle)) * speed);

            }
            
                if (Keyboard.GetState().IsKeyDown(Keys.Down))
            {
                Vector2 temp = position;
                temp -= new Vector2((float)(Math.Sin(rotationAngle)) * speed, (float)(-Math.Cos(rotationAngle)) * speed);
                if (temp.Y < graphics.PreferredBackBufferHeight - origin.Y && temp.Y > origin.Y && temp.X < graphics.PreferredBackBufferWidth - origin.X && temp.X > origin.X)
                    this.position -= new Vector2((float)(Math.Sin(rotationAngle)) * speed, (float)(-Math.Cos(rotationAngle)) * speed);
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Left))
            {
                float elapsed = (float)gameTime.ElapsedGameTime.TotalMinutes * turn_speed;
                rotationAngle -= elapsed;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Right))
            {
                float elapsed = (float)gameTime.ElapsedGameTime.TotalMinutes * turn_speed;
                rotationAngle += elapsed;
            }          
            ShellMove();
            
        }
        public void Shoot(GameTime now, KeyboardState key, KeyboardState prekey)
        {
            if(now.TotalGameTime.TotalMilliseconds - timeAfterShoot >= rechair && shells_on_baraban < 5)
            {
                shells_on_baraban++;
            }
            if (key.IsKeyDown(Keys.Space) && prekey.IsKeyUp(Keys.Space))
            {
                if(now.TotalGameTime.TotalMilliseconds - timeAfterShoot >= timeRechairBaraban && shells_on_baraban > 0)
                {
                    Vector2 direction = position + new Vector2((float)(Math.Sin(rotationAngle)) * (origin.X), (float)(-Math.Cos(rotationAngle)) * origin.Y);
                    shells.Add(new Shell(shell, shell_speed, direction, rotationAngle));
                    timeAfterShoot = (float)now.TotalGameTime.TotalMilliseconds;
                    shells_on_baraban--;
                }
            }
           
        }
        public void ShellMove()
        {
            foreach (var shell in shells)
            {
                shell.position += new Vector2((float)(Math.Sin(shell.angle)) * shell.speed, (float)(-Math.Cos(shell.angle)) * shell.speed);
            }
        }
        public void DeleteShells()
        {
            foreach (var _shell in delete_shells)
                shells.Remove(_shell);
            delete_shells = new List<Shell>();
        }
    }
}
