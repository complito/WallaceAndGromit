using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace WallaceAndGromit
{
    public enum AnimationDirection
    {
        Left,
        Right,
        Up,
        Down,
        None
    }

    public enum LocationName
    {
        Initial,
        Survival,
        Search,
        Rescue,
        None
    }

    public partial class Form1 : Form
    {
        private const int labelRange = 50;
        private const int itemsNumber = 3;
        private bool isPressedAnyKey = false;
        private bool isSurvivalPassed = false;
        private bool isSearchPassed = false;
        private bool isRescuePassed = false;
        private bool isNearToItem = false;
        private bool toUpdateAnimation = false;
        private bool toUpdateAnimationBot = false;
        private bool isLifeWasted = false;
        private LocationName previousLocation = LocationName.None;
        private LocationName currentLocation = LocationName.Initial;
        private LocationName nextLocation = LocationName.None;
        private Point cameraOffset = new Point(0, 0);
        private Player wallace;
        private List<Player> bots = new List<Player>();
        private List<Item> items = new List<Item>();
        private int frameBot = 0;
        private int life = 3;
        private int nearItemIndex;
        private int numberOfCollectedItems = 0;
        private int timeLeft = 30;
        private Map map;
        private Label labelToChangeLocation = new Label
        {
            Size = new Size(320, 15),
            Location = new Point(50, 50),
            Text = "Нажмите латинскую клавишу \"E\", чтобы изменить локацию",
            Visible = false
        };
        private Label labelToTakeItem = new Label
        {
            Size = new Size(330, 15),
            Location = new Point(600, 50),
            Text = "Нажми латинскую клавишу \"E\", чтобы подобрать часть ключа",
            Visible = false
        };
        private Label labelForTimeLeft = new Label
        {
            Size = new Size(125, 15),
            Location = new Point(1100, 50),
            Text = $"Осталось времени: 30",
            Visible = false
        };
        private string partPathImage = Path.GetFullPath("..\\..\\images\\");
        private string extension = ".png";
        private Timer timerAnimation = new Timer { Interval = 100 };
        private Timer timerMovement = new Timer { Interval = 1 };
        private Timer timerForSearch = new Timer { Interval = 1000 };

        public Form1()
        {
            InitializeComponent();
            Controls.Add(labelToChangeLocation);
            Controls.Add(labelToTakeItem);
            Controls.Add(labelForTimeLeft);
            DoubleBuffered = true;
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

            wallace = new Player(new Size(63, 100), 577, 310, 6);
            var mapLayout = new int[,] // 0 - grass, 1 - wall
            {
                { 1, 1, 1, 1, 0, 0, 1, 1, 1, 1, 1},
                { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                { 1, 1, 1, 1, 0, 0, 1, 1, 1, 1, 1}
            };
            var grassImage = new Bitmap($"{partPathImage}Grass{extension}");
            var wallImage = new Bitmap($"{partPathImage}Wall{extension}");
            map = new Map(mapLayout, 64, 64, grassImage, wallImage);

            timerAnimation.Tick += new EventHandler(UpdateAnimation);
            timerAnimation.Start();
            timerMovement.Tick += new EventHandler(UpdateMovement);
            timerMovement.Start();
            timerForSearch.Tick += new EventHandler(ReduceTime);

            KeyDown += new KeyEventHandler(Keybord);
            KeyUp += new KeyEventHandler(FreeKey);
            Paint += new PaintEventHandler(OnPaint);
        }

        private void ReduceTime(object sender, EventArgs e)
        {
            if (timeLeft > 0 && !isSearchPassed)
            {
                --timeLeft;
                labelForTimeLeft.Text = "Осталось времени: " + Convert.ToString(timeLeft);
            }
        }

        private void UpdateAnimation(object sender, EventArgs e)
        {
            toUpdateAnimation = true;
            if (currentLocation == LocationName.Survival) toUpdateAnimationBot = true;
        }

        private void UpdateMovement(object sender, EventArgs e)
        {
            if (Collide(wallace) && timeLeft != 0)
            {
                CatchUp();
                NearToItems();
                ChangeLabelVisible();
                switch (wallace.CurrentAnimation)
                {
                    case AnimationDirection.Left:
                        if (wallace.X > map.TextureWidth)
                        {
                            wallace.Left();
                            if (currentLocation != LocationName.Initial &&
                                currentLocation != LocationName.Survival && ToMoveByX())
                                cameraOffset.X += wallace.Speed;
                        }
                        break;
                    case AnimationDirection.Right:
                        if (wallace.X + wallace.Size.Width < map.TextureWidth * (map.MapLayout.GetLength(0) - 1))
                        {
                            wallace.Right();
                            if (currentLocation != LocationName.Initial &&
                                currentLocation != LocationName.Survival && ToMoveByX())
                                cameraOffset.X -= wallace.Speed;
                        }
                        break;
                    case AnimationDirection.Up:
                        if (wallace.Y > map.TextureWidth)
                        {
                            wallace.Up();
                            if (currentLocation != LocationName.Initial && ToMoveByY())
                                cameraOffset.Y += wallace.Speed;
                        }
                        break;
                    case AnimationDirection.Down:
                        if (wallace.Y + wallace.Size.Height < map.TextureHeight * (map.MapLayout.GetLength(1) - 1))
                        {
                            wallace.Down();
                            if (currentLocation != LocationName.Initial && ToMoveByY())
                                cameraOffset.Y -= wallace.Speed;
                        }
                        break;
                }
            }
            else if (!isLifeWasted)
            {
                isLifeWasted = true;
                var messageTitle = "";
                --life;
                if (life > 0)
                {
                    var messageEnd = life == 2 ? "жизни" : "жизнь";
                    if (currentLocation == LocationName.Survival)
                    {
                        messageTitle = "Пингвин не хочет тебя отпускать";
                        if (previousLocation == LocationName.Initial) currentLocation = LocationName.Initial;
                        else if (previousLocation == LocationName.Search) currentLocation = LocationName.Search;
                        nextLocation = LocationName.Survival;
                    }
                    else if (currentLocation == LocationName.Search)
                    {
                        messageTitle = "Время истекло!";
                        if (previousLocation == LocationName.Initial) currentLocation = LocationName.Initial;
                        else if (previousLocation == LocationName.Survival) currentLocation = LocationName.Survival;
                        nextLocation = LocationName.Search;
                    }
                    MessageBox.Show($"У тебя осталось {life} {messageEnd}", messageTitle);
                    ChangeLocation();
                }
                else
                {
                    if (currentLocation == LocationName.Survival) messageTitle = "Пингвины тебя больше не отпустят";
                    else if (currentLocation == LocationName.Search) messageTitle = "Время истекло!";
                    MessageBox.Show($"Игра окончена!", messageTitle);
                }
            }
            Invalidate();
        }

        private void Keybord(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode.ToString())
            {
                case "A":
                    wallace.CurrentAnimation = AnimationDirection.Left;
                    break;
                case "D":
                    wallace.CurrentAnimation = AnimationDirection.Right;
                    break;
                case "W":
                    wallace.CurrentAnimation = AnimationDirection.Up;
                    break;
                case "S":
                    wallace.CurrentAnimation = AnimationDirection.Down;
                    break;
                case "E":
                    if (labelToChangeLocation.Visible && nextLocation != LocationName.None) ChangeLocation();
                    else if (currentLocation == LocationName.Search && labelToTakeItem.Visible)
                    {
                        items.Remove(items[nearItemIndex]);
                        ++numberOfCollectedItems;
                        if (numberOfCollectedItems == itemsNumber)
                        {
                            isSearchPassed = true;
                            labelForTimeLeft.Visible = false;
                        }
                    }
                    return;
                default:
                    return;
            }
            isPressedAnyKey = true;
        }

        private void FreeKey(object sender, KeyEventArgs e)
        {
            isPressedAnyKey = false;
            wallace.CurrentFrame = 0;
            if (wallace.CurrentAnimation == AnimationDirection.Left
                || wallace.CurrentAnimation == AnimationDirection.Right)
                wallace.PreviousAnimation = wallace.CurrentAnimation;
            wallace.CurrentAnimation = AnimationDirection.None;
            Invalidate();
        }

        private void OnPaint(object sender, PaintEventArgs e)
        {
            Graphics gr = e.Graphics;
            CreateMap(gr);
            PlayAnimationMovement(gr);
            if (currentLocation == LocationName.Survival) PlayAnimationMovementBot(gr);
            else if (currentLocation == LocationName.Search) DrawItems(gr);
        }

        private void NearToItems()
        {
            bool isNear = false;
            var i = 0;
            for (; i < items.Count && !isNear; ++i)
            {
                if (wallace.X + wallace.Size.Width / 2 > items[i].X - 50 &&
                    wallace.X < items[i].X + items[i].Image.Size.Width + 50 &&
                    wallace.Y + wallace.Size.Height > items[i].Y - 50 &&
                    wallace.Y < items[i].Y + items[i].Image.Size.Height + 50)
                    isNear = true;
            }
            if (isNear)
            {
                labelToTakeItem.Visible = true;
                nearItemIndex = i - 1;
            }
            else labelToTakeItem.Visible = false;
        }

        private void DrawItems(Graphics gr)
        {
            foreach (var item in items)
                gr.DrawImage(item.Image, item.X + cameraOffset.X,
                    item.Y + cameraOffset.Y, item.Image.Size.Width, item.Image.Size.Height);
        }

        private void PlayAnimationMovement(Graphics gr)
        {
            if (isPressedAnyKey)
            {
                if (toUpdateAnimation)
                {
                    if (wallace.CurrentFrame == 3) wallace.CurrentFrame = -1;
                    ++wallace.CurrentFrame;
                    toUpdateAnimation = false;
                }
                switch (wallace.CurrentAnimation)
                {
                    case AnimationDirection.Left:
                        DrawWallace(gr, "Left");
                        break;
                    case AnimationDirection.Right:
                        DrawWallace(gr, "Right");
                        break;
                    case AnimationDirection.Up:
                        DrawLeftOrRightWallace(gr);
                        break;
                    case AnimationDirection.Down:
                        DrawLeftOrRightWallace(gr);
                        break;
                }
            }
            else DrawLeftOrRightWallace(gr);
        }

        private void DrawLeftOrRightWallace(Graphics gr)
        {
            if (wallace.PreviousAnimation == AnimationDirection.Left) DrawWallace(gr, "Left");
            else DrawWallace(gr, "Right");
        }

        private void DrawWallace(Graphics gr, string direction)
        {
            var wallaceImage = new Bitmap($"{partPathImage}Wallace_{direction}_{wallace.CurrentFrame}{extension}");
            gr.DrawImage(wallaceImage, wallace.X + cameraOffset.X,
                wallace.Y + cameraOffset.Y, wallace.Size.Width, wallace.Size.Height);
        }

        private void PlayAnimationMovementBot(Graphics gr)
        {
            if (toUpdateAnimationBot)
            {
                if (frameBot == 3) frameBot = -1;
                ++frameBot;
                toUpdateAnimationBot = false;
            }
            foreach (var bot in bots)
                DrawLeftOrRightBot(gr, bot);
        }

        private void DrawLeftOrRightBot(Graphics gr, Player bot)
        {
            if (bot.CurrentAnimation == AnimationDirection.Left) DrawBotImage(gr, "Left", bot);
            else DrawBotImage(gr, "Right", bot);
        }

        private void DrawBotImage(Graphics gr, string direction, Player bot)
        {
            Bitmap botImage;
            if (currentLocation == LocationName.Survival)
                botImage = new Bitmap($"{partPathImage}Penguin_{direction}_{frameBot}{extension}");
            else botImage = new Bitmap($"{partPathImage}Wallace_{direction}_{frameBot}{extension}");
            gr.DrawImage(botImage, bot.X + cameraOffset.X,
                bot.Y + cameraOffset.Y, bot.Size.Width, bot.Size.Height);
        }

        private void CreateMap(Graphics gr)
        {
            for (var i = 0; i < map.MapLayout.GetLength(0); ++i)
                for (var j = 0; j < map.MapLayout.GetLength(1); ++j)
                    switch (map.MapLayout[i, j])
                    {
                        case 0: // grass
                            gr.DrawImage(map.GrassImage, i * map.TextureWidth + cameraOffset.X,
                                j * map.TextureWidth + cameraOffset.Y, map.TextureWidth, map.TextureHeight);
                            break;
                        case 1: // wall
                            gr.DrawImage(map.WallImage, i * map.TextureWidth + cameraOffset.X,
                                j * map.TextureWidth + cameraOffset.Y, map.TextureWidth, map.TextureHeight);
                            break;
                    }
        }

        private void ChangeLabelVisible()
        {
            if (currentLocation == LocationName.Initial)
            {
                if (wallace.Y + wallace.Size.Height / 2 > map.TextureHeight * 4 && // left
                    wallace.Y + wallace.Size.Height / 2 < map.TextureHeight * 6 &&
                    wallace.X < map.TextureWidth + labelRange)
                    nextLocation = LocationName.Survival;
                else if (wallace.Y + wallace.Size.Height / 2 > map.TextureHeight * 4 && // right
                    wallace.Y + wallace.Size.Height / 2 < map.TextureHeight * 6 &&
                    wallace.X + wallace.Size.Width > Width - map.TextureWidth - labelRange)
                    nextLocation = LocationName.Rescue;
                else if (wallace.Y + wallace.Size.Height > map.TextureHeight * (map.MapLayout.GetLength(1) - 1) - labelRange && // down
                    wallace.X + wallace.Size.Width / 2 > map.TextureWidth * 9 &&
                    wallace.X + wallace.Size.Width / 2 < map.TextureWidth * 11)
                    nextLocation = LocationName.Search;
                else
                {
                    labelToChangeLocation.Visible = false;
                    nextLocation = LocationName.None;
                    return;
                }
                labelToChangeLocation.Visible = true;
            }
            else if (currentLocation == LocationName.Survival)
            {
                if (wallace.Y + wallace.Size.Height / 2 > map.TextureHeight * 4 && // right
                    wallace.Y + wallace.Size.Height / 2 < map.TextureHeight * 6 &&
                    wallace.X + wallace.Size.Width > Width - map.TextureWidth - labelRange)
                    nextLocation = LocationName.Initial;
                else if (wallace.Y + wallace.Size.Height > map.TextureHeight * (map.MapLayout.GetLength(1) - 1) - labelRange && // down
                    wallace.X + wallace.Size.Width / 2 > map.TextureWidth * 9 &&
                    wallace.X + wallace.Size.Width / 2 < map.TextureWidth * 11)
                    nextLocation = LocationName.Search;
                else
                {
                    labelToChangeLocation.Visible = false;
                    nextLocation = LocationName.None;
                    return;
                }
                labelToChangeLocation.Visible = true;
            }
            else if (currentLocation == LocationName.Search)
            {
                if (!isSearchPassed) labelForTimeLeft.Visible = true;
                if (wallace.Y + wallace.Size.Height / 2 > map.TextureHeight * 4 && // left
                    wallace.Y + wallace.Size.Height / 2 < map.TextureHeight * 6 &&
                    wallace.X < map.TextureWidth + labelRange)
                    nextLocation = LocationName.Survival;
                else if (wallace.Y < map.TextureHeight + labelRange && // up
                    wallace.X + wallace.Size.Width / 2 > map.TextureWidth * 4 &&
                    wallace.X + wallace.Size.Width / 2 < map.TextureWidth * 6)
                    nextLocation = LocationName.Initial;
                else if (wallace.X + wallace.Size.Width > map.TextureWidth * (map.MapLayout.GetLength(0) - 1) - labelRange && // right
                    wallace.Y + wallace.Size.Height / 2 > map.TextureHeight * 4 &&
                    wallace.Y + wallace.Size.Height / 2 < map.TextureHeight * 6 &&
                    isSearchPassed)
                    nextLocation = LocationName.Rescue;
                else
                {
                    labelToChangeLocation.Visible = false;
                    nextLocation = LocationName.None;
                    return;
                }
                labelToChangeLocation.Visible = true;
            }
            else if (currentLocation == LocationName.Rescue)
            {
                if (wallace.Y + wallace.Size.Height / 2 > map.TextureHeight * 4 && // left
                    wallace.Y + wallace.Size.Height / 2 < map.TextureHeight * 6 &&
                    wallace.X < map.TextureWidth + labelRange)
                    nextLocation = LocationName.Search;
                else if (wallace.Y < map.TextureHeight + labelRange && // up
                    wallace.X + wallace.Size.Width / 2 > map.TextureWidth * 4 &&
                    wallace.X + wallace.Size.Width / 2 < map.TextureWidth * 6)
                    nextLocation = LocationName.Initial;
                else
                {
                    labelToChangeLocation.Visible = false;
                    nextLocation = LocationName.None;
                    return;
                }
                labelToChangeLocation.Visible = true;
            }
        }

        private void ChangeLocation()
        {
            var rnd = new Random();
            isLifeWasted = false;
            switch (nextLocation)
            {
                case LocationName.Initial:
                    timerForSearch.Stop();
                    map.MapLayout = new int[,] // 0 - grass, 1 - wall
                    {
                        { 1, 1, 1, 1, 0, 0, 1, 1, 1, 1, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 1, 1, 1, 0, 0, 1, 1, 1, 1, 1}
                    };
                    switch (currentLocation)
                    {
                        case LocationName.Survival:
                            if (previousLocation == LocationName.Search && !isSurvivalPassed)
                                isSurvivalPassed = true;
                            wallace.X = map.TextureWidth + wallace.Size.Width / 2;
                            wallace.Y = map.TextureHeight * 4;
                            bots.Clear();
                            break;
                        case LocationName.Search:
                            wallace.X = map.TextureWidth * 9;
                            wallace.Y = map.TextureHeight * (map.MapLayout.GetLength(1) - 4) + wallace.Size.Height / 2;
                            break;
                        case LocationName.Rescue:
                            wallace.X = map.TextureWidth * (map.MapLayout.GetLength(0) - 3) + wallace.Size.Width / 2;
                            wallace.Y = map.TextureHeight * 4;
                            break;
                    }
                    currentLocation = LocationName.Initial;
                    cameraOffset = new Point(0, 0);
                    break;
                case LocationName.Survival:
                    timerForSearch.Stop();
                    switch (currentLocation)
                    {
                        case LocationName.Initial:
                            previousLocation = LocationName.Initial;
                            wallace.X = Width - map.TextureWidth - labelRange - wallace.Size.Width / 2;
                            wallace.Y = map.TextureHeight * 4;
                            cameraOffset.Y = 0;
                            break;
                        case LocationName.Search:
                            previousLocation = LocationName.Search;
                            wallace.X = map.TextureWidth * 9;
                            wallace.Y = map.TextureHeight * (map.MapLayout.GetLength(1) - 4) + wallace.Size.Height / 2;
                            cameraOffset.Y = -map.TextureHeight * (map.MapLayout.GetLength(1) - 11);
                            break;
                    }
                    map.MapLayout = new int[,] // 0 - grass, 1 - wall
                    {
                        { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 1, 1, 1, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1}
                    };
                    if ((currentLocation == LocationName.Initial || currentLocation == LocationName.Search) && !isSurvivalPassed)
                    {
                        bots.Clear();
                        bool isFirst3Positions = false;
                        var y = map.TextureHeight + 10;
                        var shift = 8;
                        if (y == map.TextureHeight + 10 || y == (map.TextureHeight + 10) * shift || y == (map.TextureHeight + 10) * shift * 2)
                            isFirst3Positions = true;
                        for (; y < map.TextureHeight * (map.MapLayout.GetLength(1) - 5); y += map.TextureHeight * shift)
                        {
                            var x = rnd.Next(map.TextureWidth + 10,
                                map.TextureWidth * (map.MapLayout.GetLength(0) - (isFirst3Positions ? 8 : 2)) - 10);
                            if ((x % 2 == 0 && previousLocation == LocationName.Initial) ||
                                (x % 2 != 0 && previousLocation == LocationName.Search)) ++x;
                            bots.Add(new Player(new Size(39 + 24, 66 + 24), x, y, 2));
                        }
                            
                    }
                    currentLocation = LocationName.Survival;
                    break;
                case LocationName.Search:
                    map.MapLayout = new int[,] // 0 - grass, 1 - wall
                    {
                        { 1, 1, 1, 1, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 1, 1, 1, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1}
                    };
                    switch (currentLocation)
                    {
                        case LocationName.Initial:
                            previousLocation = LocationName.Initial;
                            wallace.X = map.TextureWidth * 4;
                            wallace.Y = map.TextureHeight + wallace.Size.Height / 2;
                            break;
                        case LocationName.Survival:
                            if (previousLocation == LocationName.Initial && !isSurvivalPassed)
                                isSurvivalPassed = true;
                            previousLocation = LocationName.Survival;
                            wallace.X = map.TextureWidth + wallace.Size.Width / 2;
                            wallace.Y = map.TextureHeight * 4;
                            bots.Clear();
                            break;
                        case LocationName.Rescue:
                            previousLocation = LocationName.Rescue;
                            wallace.X = map.TextureWidth * (map.MapLayout.GetLength(0) - 3) + wallace.Size.Width / 2;
                            wallace.Y = map.TextureHeight * 4;
                            break;
                    }
                    if (currentLocation == LocationName.Rescue)
                        cameraOffset = new Point(-map.TextureHeight * (map.MapLayout.GetLength(0) - 20), 0);
                    else cameraOffset = new Point(0, 0);
                    numberOfCollectedItems = 0;
                    timeLeft = 30;
                    labelForTimeLeft.Text = $"Осталось времени: {timeLeft}";
                    timerForSearch.Start();
                    if (!isSearchPassed)
                    {
                        items.Clear();
                        for (var i = 0; i < itemsNumber; ++i)
                        {
                            var x = rnd.Next(map.TextureWidth + 10, map.TextureWidth * (map.MapLayout.GetLength(0) - 2) - 10);
                            var y = rnd.Next(map.TextureHeight + 10, map.TextureHeight * (map.MapLayout.GetLength(1) - 2) - 10);
                            items.Add(new Item(x, y, new Bitmap($"{partPathImage}Key_{i}{extension}")));
                        }
                    }
                    currentLocation = LocationName.Search;
                    break;
                case LocationName.Rescue:
                    timerForSearch.Stop();
                    map.MapLayout = new int[,] // 0 - grass, 1 - wall
                    {
                        { 1, 1, 1, 1, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                        { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1}
                    };
                    switch (currentLocation)
                    {
                        case LocationName.Initial:
                            wallace.X = map.TextureWidth * 4;
                            wallace.Y = map.TextureHeight + wallace.Size.Height / 2;
                            break;
                        case LocationName.Search:
                            wallace.X = map.TextureWidth + wallace.Size.Width / 2;
                            wallace.Y = map.TextureHeight * 4;
                            break;
                    }
                    currentLocation = LocationName.Rescue;
                    cameraOffset = new Point(0, 0);
                    break;
            }
        }

        private bool ToMoveByY()
        {
            return wallace.Y > map.TextureHeight * 4 &&
                wallace.Y + wallace.Size.Height < map.TextureHeight * (map.MapLayout.GetLength(1) - 5);
        }

        public bool ToMoveByX()
        {
            return wallace.X > map.TextureWidth * 10 &&
                wallace.X + wallace.Size.Width < map.TextureWidth * (map.MapLayout.GetLength(0) - 9);
        }

        private bool Collide(Player player)
        {
            foreach (var bot in bots)
            {
                if (bot.Y <= player.Y + player.Size.Height && // down
                    bot.Y >= player.Y + player.Size.Height / 2 &&
                    bot.X + bot.Size.Width >= player.X &&
                    bot.X <= player.X + player.Size.Width)
                    return false;
                if (bot.X + bot.Size.Width >= player.X && // left
                    bot.X + bot.Size.Width <= player.X + player.Size.Width / 2 &&
                    bot.Y + bot.Size.Height >= player.Y &&
                    bot.Y <= player.Y + player.Size.Height)
                    return false;
                if (bot.Y + bot.Size.Height >= player.Y && // up
                    bot.Y + bot.Size.Height <= player.Y + player.Size.Height / 2 &&
                    bot.X + bot.Size.Width >= player.X &&
                    bot.X <= player.X + player.Size.Width)
                    return false;
                if (bot.X <= player.X + player.Size.Width && // right
                    bot.X >= player.X + player.Size.Width / 2 &&
                    bot.Y + bot.Size.Height >= player.Y &&
                    bot.Y <= player.Y + player.Size.Height)
                    return false;
            }
            return true;
        }

        private void CatchUp()
        {
            //bool toMoveBot = true;
            for (var i = 0; i < bots.Count; ++i)
            {
                //for (var j = 0; j < bots.Count; ++j)
                //{
                //    if (j == i) continue;
                //    toMoveBot = Collide(bots[j]);
                //}
                bool isChangedY = false;
                bool isChangedX = false;
                if (bots[i].X == wallace.X)
                {
                    if (bots[i].Y > wallace.Y/* && toMoveBot*/)
                        bots[i].Y -= bots[i].Speed;
                    else if (bots[i].Y < wallace.Y/* && toMoveBot*/)
                        bots[i].Y += bots[i].Speed;
                    isChangedY = true;
                }
                else
                {
                    if (bots[i].X > wallace.X/* && toMoveBot*/)
                    {
                        bots[i].X -= bots[i].Speed;
                        bots[i].CurrentAnimation = AnimationDirection.Left;
                    }
                    else if (bots[i].X < wallace.X/* && toMoveBot*/)
                    {
                        bots[i].X += bots[i].Speed;
                        bots[i].CurrentAnimation = AnimationDirection.Right;
                    }
                    isChangedX = true;
                }
                if (bots[i].Y == wallace.Y)
                {
                    //toMoveBot = true;
                    //for (var j = 0; j < bots.Count; ++j)
                    //{
                    //    if (j == i) continue;
                    //    toMoveBot = Collide(bots[j]);
                    //}
                    if (!isChangedX)
                    {
                        if (bots[i].X > wallace.X/* && toMoveBot*/)
                        {
                            bots[i].X -= bots[i].Speed;
                            bots[i].CurrentAnimation = AnimationDirection.Left;
                        }
                        else if (bots[i].X < wallace.X/* && toMoveBot*/)
                        {
                            bots[i].X += bots[i].Speed;
                            bots[i].CurrentAnimation = AnimationDirection.Right;
                        }
                    }
                }
                else
                {
                    if (!isChangedY)
                    {
                        if (bots[i].Y > wallace.Y/* && toMoveBot*/)
                            bots[i].Y -= bots[i].Speed;
                        else if (bots[i].Y < wallace.Y/* && toMoveBot*/)
                            bots[i].Y += bots[i].Speed;
                    }
                }
            }
        }
    }
}
