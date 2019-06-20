using LazerTanks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Game1
{
    public class Main : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont mainFont;
        Tank tank;
        //Параметры танка
        float rechair = 2000;
        float speed = 5;
        float angle = 90;
        int HP = 5;
        float turn_speed = 200;
        float shell_speed = 10;

        public Main()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = screenWidht = 1200;
            graphics.PreferredBackBufferHeight = screenHeight = 600;
            graphics.IsFullScreen = true;
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            mainFont = Content.Load<SpriteFont>("Main");
        }

        protected override void UnloadContent()
        {

        }
        GameState gameState = GameState.Menu;
        MenuOptions option = MenuOptions.CreateGame;

        int OptionCounter = 1;
        int optionsCounter
        {
            get
            {
                return OptionCounter;
            }
            set
            {
                if (value > 3)
                    OptionCounter = 3;
                if (value < 1)
                    OptionCounter = 1;

                if (OptionCounter == 1)
                    option = MenuOptions.CreateGame;
                if (OptionCounter == 2)
                    option = MenuOptions.EnterGame;
                if (OptionCounter == 3)
                    option = MenuOptions.Exit;
            }
        }
        KeyboardState key;
        KeyboardState prekey;
        int screenWidht;
        int screenHeight;
        string gameCreate = "Create";
        string gameEnter = "Enter";
        string gameExit = "Exit";
        //Переменный для ожидание сервера
        Thread tRec;//Поток для принятие данных от клиентов
        Thread gameServerRec;
        bool start_tRec = false;
        bool start_gameServerRec = false;
        int quantuityClient = 0;
        //Переменный для ожидание клиента
        Thread cRec;//поток для передачи и принятие сообщение
        Thread gameClientRec;
        bool start_cRec = false;
        bool start_gameClientRec = false;
        int countDown;
        GameTime gameSeconds;
        protected override void Update(GameTime gameTime)
        {
            gameSeconds = gameTime;
            key = Keyboard.GetState();
            if (key.IsKeyDown(Keys.Escape) && prekey.IsKeyUp(Keys.Escape))
            {
                if (gameState == GameState.GameClient || gameState == GameState.GameServer)
                {
                    if (start_cRec)
                    {
                        cRec.Abort();
                        start_cRec = false;
                    }
                    if (start_tRec)
                    {
                        tRec.Abort();
                        start_tRec = false;
                    }
                    if(start_gameClientRec)
                    {
                        gameClientRec.Abort();
                        start_gameClientRec = false;
                        Client.tanks.ElementAt(Client.slot).alive = false;
                    }
                    if (start_gameServerRec)
                    {
                        gameServerRec.Abort();
                        start_gameServerRec = false;
                        Server.tanks.ElementAt(0).alive = false;
                    }
                    gameState = GameState.Menu;
                }
            }
            switch (gameState)
            {
                case GameState.Menu:
                    if (key.IsKeyDown(Keys.Up) && prekey.IsKeyUp(Keys.Up))
                    {
                        OptionCounter--;
                        optionsCounter = OptionCounter;
                    }
                    if (key.IsKeyDown(Keys.Down) && prekey.IsKeyUp(Keys.Down))
                    {
                        OptionCounter++;
                        optionsCounter = OptionCounter;
                    }
                    if (key.IsKeyDown(Keys.Enter) && prekey.IsKeyUp(Keys.Enter))
                    {
                        switch (option)
                        {
                            case MenuOptions.CreateGame:
                                gameState = GameState.ExpectationServer;
                                break;
                            case MenuOptions.EnterGame:
                                gameState = GameState.ConnectedClient;
                                break;
                            case MenuOptions.Exit:
                                Exit();
                                break;
                        }
                    }
                    break;
                case GameState.ExpectationServer:
                    if (Server.numConnectedClients < 3)
                    {
                        if (!start_tRec)
                        {
                            Server.tanks.Add(new Tank(Content.Load<Texture2D>("Tank_green"), Content.Load<Texture2D>("g"), new Vector2(30, 30), rechair, shell_speed, speed, angle, HP, turn_speed, graphics));
                            //Запускаем поток принятие сообщений
                            tRec = new Thread(new ThreadStart(Server.ReceiverServer));
                            tRec.Start();
                            start_tRec = true;
                        }
                        if (Server.numConnectedClients != quantuityClient)
                        {
                            quantuityClient++;
                            switch (Server.numConnectedClients)
                            {
                                case 1:
                                    Server.tanks.Add(new Tank(Content.Load<Texture2D>("Tank_yellow"), Content.Load<Texture2D>("y"), new Vector2(screenWidht - 30, 30), rechair, shell_speed, speed, angle, HP, turn_speed, graphics));
                                    break;
                                case 2:
                                    Server.tanks.Add(new Tank(Content.Load<Texture2D>("Tank_red"), Content.Load<Texture2D>("r"), new Vector2(30, screenHeight - 30), rechair, shell_speed, speed, angle, HP, turn_speed, graphics));
                                    break;
                                case 3:
                                    Server.tanks.Add(new Tank(Content.Load<Texture2D>("Tank_blue"), Content.Load<Texture2D>("b"), new Vector2(screenWidht - 30, screenHeight - 30), rechair, shell_speed, speed, angle, HP, turn_speed, graphics));
                                    break;
                            }
                        }
                    }
                    for (int i = 0; i < Server.numConnectedClients; i++)
                    {
                        if (Server.AddressClient[i] != null)
                            Server.SendSlot(Server.AddressClient[i]);
                    }
                    if (key.IsKeyDown(Keys.Enter) && prekey.IsKeyUp(Keys.Enter))
                    {
                        tRec.Abort();
                        start_tRec = false;
                        gameState = GameState.Countdown;
                        countDown = gameTime.TotalGameTime.Seconds;
                        Server.InitializateJsonTanks();
                        for (int i = 0; i < Server.numConnectedClients; i++)
                            Server.SendStartGame(Server.AddressClient[i], "start");
                    }
                    break;
                case GameState.ConnectedClient:
                    if (Client.connect)
                    {
                        if (!start_cRec)
                        {
                            cRec = new Thread(new ThreadStart(Client.ReceiverClient));
                            cRec.Start();
                            start_cRec = true;
                        }
                        if (Client.slot != 0)
                        {
                            Client.tanks.Add(new Tank(Content.Load<Texture2D>("Tank_green"), Content.Load<Texture2D>("g"), new Vector2(30, 30), rechair, shell_speed, speed, angle, HP, turn_speed, graphics));
                            switch (Client.slot)
                            {
                                case 1:
                                    Client.tanks.Add(new Tank(Content.Load<Texture2D>("Tank_yellow"), Content.Load<Texture2D>("y"), new Vector2(screenWidht - 30, 30), rechair, shell_speed, speed, angle, HP, turn_speed, graphics));
                                    break;
                                case 2:
                                    Client.tanks.Add(new Tank(Content.Load<Texture2D>("Tank_red"), Content.Load<Texture2D>("r"), new Vector2(30, screenHeight - 30), rechair, shell_speed, speed, angle, HP, turn_speed, graphics));
                                    Client.tanks.Add(new Tank(Content.Load<Texture2D>("Tank_yellow"), Content.Load<Texture2D>("y"), new Vector2(screenWidht - 30, 30), rechair, shell_speed, speed, angle, HP, turn_speed, graphics));
                                    break;
                                case 3:
                                    Client.tanks.Add(new Tank(Content.Load<Texture2D>("Tank_blue"), Content.Load<Texture2D>("b"), new Vector2(screenWidht - 30, screenHeight - 30), rechair, shell_speed, speed, angle, HP, turn_speed, graphics));
                                    Client.tanks.Add(new Tank(Content.Load<Texture2D>("Tank_red"), Content.Load<Texture2D>("r"), new Vector2(30, screenHeight - 30), rechair, shell_speed, speed, angle, HP, turn_speed, graphics));
                                    Client.tanks.Add(new Tank(Content.Load<Texture2D>("Tank_yellow"), Content.Load<Texture2D>("y"), new Vector2(screenWidht - 30, 30), rechair, shell_speed, speed, angle, HP, turn_speed, graphics));
                                    break;
                            }
                            gameState = GameState.ExpectationClient;
                        }
                    }
                    else
                    {
                        Client.Send();
                    }
                    break;
                case GameState.ExpectationClient:
                    if (Client.InsertSlot != 0 && Client.tanks.Count - 1 < Client.InsertSlot)
                    {
                        switch (Convert.ToInt32(Client.InsertSlot))
                        {
                            case 2:
                                Client.tanks.Add(new Tank(Content.Load<Texture2D>("Tank_red"), Content.Load<Texture2D>("r"), new Vector2(30, screenHeight - 30), rechair, shell_speed, speed, angle, HP, turn_speed, graphics));
                                break;
                            case 3:
                                Client.tanks.Add(new Tank(Content.Load<Texture2D>("Tank_blue"), Content.Load<Texture2D>("b"), new Vector2(screenWidht - 30, screenHeight - 30), rechair, shell_speed, speed, angle, HP, turn_speed, graphics));
                                break;
                        }
                        Client.InsertSlot = 0;
                    }
                    Client.Send();
                    if (Client.startGame)
                    {
                        cRec.Abort();
                        start_cRec = false;
                        Client.InitializateJsonTanks();
                        gameState = GameState.Countdown;
                        countDown = gameTime.TotalGameTime.Seconds;
                    }
                    break;
                case GameState.GameServer:
                    if(!start_gameServerRec)
                    {
                        gameServerRec = new Thread(new ThreadStart(Server.ReceiverTank));
                        gameServerRec.Start();
                        start_gameServerRec = true;
                    }
                    tank = Server.tanks.ElementAt(0);
                    if(tank.alive)
                    {
                        tank.Move(gameTime);
                        tank.Shoot(gameTime, key, prekey);
                    }
                    Server.tanks.ElementAt(0).HP = Int32.Parse(Server.jsonTanks.tanks.ElementAt(0).HP);
                    for (int i = 1; i < Server.jsonTanks.number; i++)
                    {
                        JsonTank tank = Server.jsonTanks.tanks.ElementAt(i);
                        Server.tanks.ElementAt(i).HP = Int32.Parse(Server.jsonTanks.tanks.ElementAt(i).HP);
                        Server.tanks.ElementAt(i).rotationAngle = Convert.ToSingle(Server.jsonTanks.tanks.ElementAt(i).rotationAngle);
                        Server.tanks.ElementAt(i).position = new Vector2(Convert.ToSingle(Server.jsonTanks.tanks.ElementAt(i).positionX), Convert.ToSingle(Server.jsonTanks.tanks.ElementAt(i).positionY));
                        Server.tanks.ElementAt(i).shells_on_baraban = Int32.Parse(tank.shellOnBaraban);
                        Server.tanks.ElementAt(i).shells = new List<Shell>();
                        for (int j = 0; j < tank.shells.Count(); j++)
                            Server.tanks.ElementAt(i).shells.Add(new Shell(Server.tanks.ElementAt(i).shell, Server.tanks.ElementAt(i).shell_speed, new Vector2(Convert.ToSingle(tank.shells.ElementAt(j).positionX), Convert.ToSingle(tank.shells.ElementAt(j).positionY)), Convert.ToSingle(tank.shells.ElementAt(j).angle)));
                    }
                    foreach (var Tank in Server.tanks)
                    {
                        foreach (var shell in Tank.shells)
                            foreach (var InTank in Server.tanks)
                            {
                                if (shell.Hit(InTank) && tank != InTank)
                                {
                                    tank.delete_shells.Add(shell);
                                    InTank.HP--;
                                    break;
                                }
                            }
                        Tank.DeleteShells();
                    }
                    Server.DeleteTank();
                    Server.jsonTanks.number = Server.tanks.Count;
                    for (int i = 0; i < Server.tanks.Count; i++)
                    {
                        Tank tank = Server.tanks.ElementAt(i);
                        List<JsonShell> shells = new List<JsonShell>();
                        for (int j = 0; j < tank.shells.Count; j++)
                        {
                            shells.Add(new JsonShell()
                            {
                                positionX = tank.shells.ElementAt(j).position.X.ToString(),
                                positionY = tank.shells.ElementAt(j).position.Y.ToString(),
                                angle = tank.shells.ElementAt(j).angle.ToString()
                            });
                        }
                        Server.jsonTanks.tanks[i] = new JsonTank()
                        {
                            slot = i.ToString(),
                            HP = tank.HP.ToString(),
                            rotationAngle = tank.rotationAngle.ToString(),
                            positionX = tank.position.X.ToString(),
                            positionY = tank.position.Y.ToString(),
                            shellOnBaraban = tank.shells_on_baraban.ToString(),
                            shells = shells                           
                        };
                    }
                    string serialized = JsonConvert.SerializeObject(Server.jsonTanks);
                    for (int i = 0; i < Server.numConnectedClients; i++)
                        Server.SendTanks(Server.AddressClient[i], serialized);
                    break;
                case GameState.GameClient:
                    if (!start_gameClientRec)
                    {
                        gameClientRec = new Thread(new ThreadStart(Client.ReceiverTanks));
                        gameClientRec.Start();
                        start_gameClientRec = true;
                    }
                    tank = Client.tanks.ElementAt(Client.slot);
                    if(tank.alive)
                    {
                        tank.Move(gameTime);
                        tank.Shoot(gameTime, key, prekey);
                    }
                    Client.SendTank(Client.addressServer);
                    for (int i = 0; i < Client.jsonTanks.number; i++)
                    {
                        JsonTank tank = Client.jsonTanks.tanks.ElementAt(i);
                        Client.tanks.ElementAt(i).HP = Int32.Parse(tank.HP);
                        if (i != Client.slot)
                        {
                            Client.tanks.ElementAt(i).shells = new List<Shell>();
                            Client.tanks.ElementAt(i).rotationAngle = Convert.ToSingle(tank.rotationAngle);
                            Client.tanks.ElementAt(i).position = new Vector2(Convert.ToSingle(tank.positionX), Convert.ToSingle(tank.positionY));
                            Client.tanks.ElementAt(i).shells_on_baraban = Int32.Parse(tank.shellOnBaraban);
                            for(int j = 0; j < tank.shells.Count(); j++)
                            {
                                Client.tanks.ElementAt(i).shells.Add(new Shell(Client.tanks.ElementAt(i).shell, Client.tanks.ElementAt(i).shell_speed,new Vector2(Convert.ToSingle(tank.shells.ElementAt(j).positionX), Convert.ToSingle(tank.shells.ElementAt(j).positionY)), Convert.ToSingle(tank.shells.ElementAt(j).angle)));
                            }
                        }
                    }
                    foreach (var Tank in Client.tanks)
                    {
                        foreach (var shell in Tank.shells)
                            foreach (var InTank in Client.tanks)
                            {
                                if (shell.Hit(InTank) && tank != InTank)
                                {
                                    tank.delete_shells.Add(shell);
                                    InTank.HP--;
                                    break;
                                }
                            }
                        Tank.DeleteShells();
                    }
                    Server.jsonTanks.number = Client.tanks.Count;
                    for (int i = 0; i < Client.tanks.Count; i++)
                    {
                        Tank tank = Client.tanks.ElementAt(i);
                        List<JsonShell> shells = new List<JsonShell>();
                        for (int j = 0; j < tank.shells.Count; j++)
                        {
                            shells.Add(new JsonShell()
                            {
                                positionX = tank.shells.ElementAt(j).position.X.ToString(),
                                positionY = tank.shells.ElementAt(j).position.Y.ToString(),
                                angle = tank.shells.ElementAt(j).angle.ToString()
                            });
                        }
                        Server.jsonTanks.tanks[i] = new JsonTank()
                        {
                            slot = i.ToString(),
                            HP = tank.HP.ToString(),
                            rotationAngle = tank.rotationAngle.ToString(),
                            positionX = tank.position.X.ToString(),
                            positionY = tank.position.Y.ToString(),
                            shellOnBaraban = tank.shells_on_baraban.ToString(),
                            shells = shells
                        };
                    }
                    string sserialized = JsonConvert.SerializeObject(Client.jsonTanks);
                    Client.SendTanks(Client.addressServer, sserialized);
                    Client.DeleteTank();
                    break;
                case GameState.Countdown:
                    if(gameTime.TotalGameTime.Seconds - 3 == countDown && Server.numConnectedClients != 0)
                    {
                        gameState = GameState.GameServer;
                    }
                    if (gameTime.TotalGameTime.Seconds - 3 == countDown && Client.connect)
                    {
                        gameState = GameState.GameClient;
                    }
                    break;
            }

            prekey = key;

            base.Update(gameTime);
        }
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.White);

            // отрисовка спрайта
            spriteBatch.Begin();
            switch (gameState)
            {
                case GameState.Menu:
                    if (option == MenuOptions.CreateGame)
                    {
                        spriteBatch.DrawString(mainFont,
                                         gameCreate,
                                         new Vector2((int)screenWidht * 0.40f, (int)screenHeight * 0.40f),
                                         Color.Blue);
                    }
                    else
                    {
                        spriteBatch.DrawString(mainFont,
                                         gameCreate,
                                         new Vector2((int)screenWidht * 0.40f, (int)screenHeight * 0.40f),
                                         Color.Black);
                    }
                    if (option == MenuOptions.EnterGame)
                    {
                        spriteBatch.DrawString(mainFont,
                                         gameEnter,
                                         new Vector2((int)screenWidht * 0.40f, (int)screenHeight * 0.60f),
                                         Color.Blue);
                    }
                    else
                    {
                        spriteBatch.DrawString(mainFont,
                                         gameEnter,
                                         new Vector2((int)screenWidht * 0.40f, (int)screenHeight * 0.60f),
                                         Color.Black);
                    }
                    if (option == MenuOptions.Exit)
                    {
                        spriteBatch.DrawString(mainFont,
                                         gameExit,
                                         new Vector2((int)screenWidht * 0.40f, (int)screenHeight * 0.80f),
                                         Color.Blue);
                    }
                    else
                    {
                        spriteBatch.DrawString(mainFont,
                                         gameExit,
                                         new Vector2((int)screenWidht * 0.40f, (int)screenHeight * 0.80f),
                                         Color.Black);
                    }
                    break;
                case GameState.ExpectationServer:
                    for (int i = 0; i < Server.numConnectedClients; i++)
                    {
                        spriteBatch.DrawString(mainFont,
                                       Server.AddressClient[i],
                                       new Vector2((int)screenWidht * 0.40f, (int)screenHeight * 0.20f * i),
                                       Color.Black);
                    }

                    foreach (var Tank in Server.tanks)
                    {
                        spriteBatch.Draw(Tank.sprite, Tank.position, null, Color.White, Tank.rotationAngle, Tank.origin, 1.0f, SpriteEffects.None, 0f);   
                    }  
                    break;
                case GameState.ConnectedClient:
                    foreach (var Tank in Client.tanks)
                    {
                        spriteBatch.Draw(Tank.sprite, Tank.position, null, Color.White, Tank.rotationAngle, Tank.origin, 1.0f, SpriteEffects.None, 0f);
                    }
                    break;
                case GameState.ExpectationClient:
                    for (int i = 0; i < Server.numConnectedClients; i++)
                    {
                        spriteBatch.DrawString(mainFont,
                                       gameState.ToString(),
                                       new Vector2((int)screenWidht * 0.40f, (int)screenHeight * 0.20f * i),
                                       Color.Black);
                    }
                    foreach (var Tank in Client.tanks)
                    {
                        spriteBatch.Draw(Tank.sprite, Tank.position, null, Color.White, Tank.rotationAngle, Tank.origin, 1.0f, SpriteEffects.None, 0f);
                    }
                    break;
                case GameState.GameServer:
                    foreach (var Tank in Server.tanks)
                    {
                        if(Tank.alive)
                        {
                            spriteBatch.Draw(Tank.sprite, Tank.position, null, Color.White, Tank.rotationAngle, Tank.origin, 1.0f, SpriteEffects.None, 0f);
                            foreach (var shell in Tank.shells)
                            {
                                spriteBatch.Draw(shell.sprite, shell.position, Color.White);
                            }
                        }   
                    }           
                    break;
                case GameState.GameClient:
                    foreach (var Tank in Client.tanks)
                    {
                        if (Tank.alive)
                        {
                            spriteBatch.Draw(Tank.sprite, Tank.position, null, Color.White, Tank.rotationAngle, Tank.origin, 1.0f, SpriteEffects.None, 0f);
                            foreach (var shell in Tank.shells)
                            {
                                spriteBatch.Draw(shell.sprite, shell.position, Color.White);
                            }
                        }
                    }
                    break;
                case GameState.Countdown:
                    Texture2D countDownTexture;
                    if (gameSeconds.TotalGameTime.Seconds - countDown < 1)
                    {
                        countDownTexture = Content.Load<Texture2D>("menu_3");
                        spriteBatch.Draw(countDownTexture, new Vector2(screenWidht / 2 - (countDownTexture.Width / 2), screenHeight / 2 - (countDownTexture.Height / 2)), Color.White);
                    }
                    else
                    {
                        if (gameSeconds.TotalGameTime.Seconds - countDown < 2)
                        {
                            countDownTexture = Content.Load<Texture2D>("menu_2");
                            spriteBatch.Draw(countDownTexture, new Vector2(screenWidht / 2 - (countDownTexture.Width / 2), screenHeight / 2 - (countDownTexture.Height / 2)), Color.White);
                        }
                        else
                        {
                            if (gameSeconds.TotalGameTime.Seconds - countDown < 4)
                            {
                                countDownTexture = Content.Load<Texture2D>("menu_1");
                                spriteBatch.Draw(countDownTexture, new Vector2(screenWidht / 2 - (countDownTexture.Width / 2), screenHeight / 2 - (countDownTexture.Height / 2)), Color.White);
                            }
                        }
                    }
                    break;
            }
            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}