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
using System.Media;
using System.Windows.Input;

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
        GamePassed,
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
        private bool toUpdateAnimation = false;
        private bool toUpdateAnimationBot = false;
        private bool isLifeWasted = false;
        private bool isAbilityUsed = false;
        private bool toUseE = true;
        private LocationName previousLocation = LocationName.None;
        private LocationName currentLocation = LocationName.Initial;
        private LocationName nextLocation = LocationName.None;
        private MapLayouts mapLayouts = new MapLayouts();
        private Point cameraOffset = new Point(0, 0);
        private Point initialWallaceLocation = new Point(1280 / 2 - 63 / 2, 720 / 2 - 100 / 2);
        private Player wallace;
        private List<Player> bots = new List<Player>();
        private List<Item> items = new List<Item>();
        private int frameBot = 0;
        private int life = 3;
        private int nearItemIndex;
        private int numberOfCollectedItems = 0;
        private const int time = 40;
        private int timeLeft;
        private Map map;
        private string pathToMusic = Path.GetFullPath("..\\..\\") + "Dee Yan-Key - ragtop.wav";
        private WMPLib.WindowsMediaPlayer WMP = new WMPLib.WindowsMediaPlayer();
        private Point[] pointsToMovePandas = new Point[]
        {
            new Point(100, 100),
            new Point(300, 100),
            new Point(500, 100)
        };
        private Label labelToChangeLocation = new Label
        {
            Location = new Point(10, 10),
            Text = "Нажмите латинскую клавишу \"E\", чтобы изменить локацию",
            BackColor = Color.Transparent,
            AutoSize = true,
            ForeColor = Color.White,
            Font = new Font("Arial", 14, FontStyle.Bold),
            Visible = false
        };
        private Label labelToTakeItem = new Label
        {
            BackColor = Color.Transparent,
            AutoSize = true,
            ForeColor = Color.White,
            Font = new Font("Arial", 14, FontStyle.Bold),
            Location = new Point(10, 10),
            Text = "Нажми латинскую клавишу \"E\", чтобы подобрать часть ключа",
            Visible = false
        };
        private Label labelForTimeLeft = new Label
        {
            BackColor = Color.Transparent,
            AutoSize = true,
            ForeColor = Color.White,
            Font = new Font("Arial", 14, FontStyle.Bold),
            Location = new Point(1025, 10),
            Visible = false
        };
        private Label labelForAbility = new Label
        {
            BackColor = Color.Transparent,
            AutoSize = true,
            ForeColor = Color.White,
            Font = new Font("Arial", 14, FontStyle.Bold),
            Location = new Point(10, 10),
            Text = "Нажмите латинскую клавишу \"E\", чтобы узнать, где одна из частей ключа",
            Visible = false
        };
        private string partPathImage = Path.GetFullPath("..\\..\\images\\");
        private string extension = ".png";
        private Timer timerAnimation = new Timer { Interval = 100 };
        private Timer timerMovement = new Timer { Interval = 1 };
        private Timer timerForSearch = new Timer { Interval = 1000 };
        private Timer timerForKeyDown = new Timer { Interval = 10 };
        private Timer timerForKeyUp = new Timer { Interval = 10 };
        private Timer timerToUseE = new Timer { Interval = 500 };

        public Form1()
        {
            InitializeComponent();
            timeLeft = time;
            labelForTimeLeft.Text = $"Осталось времени: {timeLeft}";
            WMP.URL = pathToMusic;
            WMP.settings.volume = 100;
            WMP.settings.autoStart = true;
            WMP.controls.play();
            Controls.Add(labelToChangeLocation);
            Controls.Add(labelToTakeItem);
            Controls.Add(labelForTimeLeft);
            Controls.Add(labelForAbility);
            DoubleBuffered = true;
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

            wallace = new Player(new Size(63, 100), initialWallaceLocation.X, initialWallaceLocation.Y, 6);
            var mapLayout = mapLayouts.InitialLayout;
            var grassImage = new Bitmap($"{partPathImage}Grass{extension}");
            var wallImage = new Bitmap($"{partPathImage}Wall{extension}");
            var grassWithPointImage = new Bitmap($"{partPathImage}Grass_With_Point{extension}");
            map = new Map(mapLayout, 64, 64, grassImage, wallImage, grassWithPointImage);

            timerAnimation.Tick += new EventHandler(UpdateAnimation);
            timerAnimation.Start();
            timerMovement.Tick += new EventHandler(UpdateMovement);
            timerMovement.Start();
            timerForSearch.Tick += new EventHandler(ReduceTime);
            timerForKeyDown.Tick += new EventHandler(Keybord);
            timerForKeyDown.Start();
            timerToUseE.Tick += new EventHandler(UseE);

            Paint += new PaintEventHandler(OnPaint);
        }

        private void UseE(object sender, EventArgs e)
        {
            toUseE = true;
            timerToUseE.Stop();
        }

        private void ReduceTime(object sender, EventArgs e)
        {
            if (timeLeft > 0 && ((currentLocation == LocationName.Search && !isSearchPassed) ||
               (currentLocation == LocationName.Rescue && !isRescuePassed)))
            {
                --timeLeft;
                labelForTimeLeft.Text = "Осталось времени: " + Convert.ToString(timeLeft);
            }
        }

        private void UpdateAnimation(object sender, EventArgs e)
        {
            toUpdateAnimation = true;
            toUpdateAnimationBot = true;
            UpdatePandaAnimation();
        }

        private void UpdatePandaAnimation()
        {
            for (var i = 0; i < items.Count; ++i)
            {
                if (items[i].CurrentFrame == 3) items[i].CurrentFrame = -1;
                ++items[i].CurrentFrame;
                if ((items[i].X == pointsToMovePandas[i].X && items[i].Y == pointsToMovePandas[i].Y) ||
                    !items[i].IsRescued)
                    items[i].CurrentFrame = 0;
            }
        }

        private void UpdateMovement(object sender, EventArgs e)
        {
            if (isSurvivalPassed && isSearchPassed && isRescuePassed && currentLocation == LocationName.Rescue)
            {
                map.MapLayout[map.MapLayout.GetLength(0) - 1, 4] = 0;
                map.MapLayout[map.MapLayout.GetLength(0) - 1, 5] = 0;
            }
            if (Collide(wallace) && timeLeft > 0)
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
                                currentLocation != LocationName.Survival &&
                                currentLocation != LocationName.GamePassed &&
                                ToMoveByX())
                                cameraOffset.X += wallace.Speed;
                        }
                        break;
                    case AnimationDirection.Right:
                        if (wallace.X + wallace.Size.Width < map.TextureWidth * (map.MapLayout.GetLength(0) - 1))
                        {
                            wallace.Right();
                            if (currentLocation != LocationName.Initial &&
                                currentLocation != LocationName.Survival &&
                                currentLocation != LocationName.GamePassed &&
                                ToMoveByX())
                                cameraOffset.X -= wallace.Speed;
                        }
                        break;
                    case AnimationDirection.Up:
                        if (wallace.Y > map.TextureWidth)
                        {
                            wallace.Up();
                            if (currentLocation != LocationName.Initial &&
                                currentLocation != LocationName.GamePassed &&
                                ToMoveByY())
                                cameraOffset.Y += wallace.Speed;
                        }
                        break;
                    case AnimationDirection.Down:
                        if (wallace.Y + wallace.Size.Height < map.TextureHeight * (map.MapLayout.GetLength(1) - 1))
                        {
                            wallace.Down();
                            if (currentLocation != LocationName.Initial &&
                                currentLocation != LocationName.GamePassed &&
                                ToMoveByY())
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
                    else if (currentLocation == LocationName.Rescue)
                    {
                        messageTitle = "Время истекло!";
                        if (previousLocation == LocationName.Initial) currentLocation = LocationName.Initial;
                        else if (previousLocation == LocationName.Search) currentLocation = LocationName.Search;
                        nextLocation = LocationName.Rescue;
                    }
                    MessageBox.Show($"У тебя осталось {life} {messageEnd}", messageTitle);
                    ChangeLocation();
                }
                else
                {
                    if (currentLocation == LocationName.Survival) messageTitle = "Пингвины тебя больше не отпустят";
                    else if (currentLocation == LocationName.Search || currentLocation == LocationName.Rescue)
                        messageTitle = "Время истекло!";
                    MessageBox.Show($"Игра окончена!", messageTitle);
                }
            }
            Invalidate();
        }

        private void Keybord(object sender, EventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.A))
            {
                wallace.CurrentAnimation = AnimationDirection.Left;
                isPressedAnyKey = true;
            }
            else if (Keyboard.IsKeyDown(Key.D))
            {
                wallace.CurrentAnimation = AnimationDirection.Right;
                isPressedAnyKey = true;
            }
                
            else if (Keyboard.IsKeyDown(Key.W))
            {
                wallace.CurrentAnimation = AnimationDirection.Up;
                isPressedAnyKey = true;
            }
                
            else if (Keyboard.IsKeyDown(Key.S))
            {
                wallace.CurrentAnimation = AnimationDirection.Down;
                isPressedAnyKey = true;
            }
                
            else if (Keyboard.IsKeyDown(Key.E) && toUseE)
            {
                if (labelToChangeLocation.Visible && nextLocation != LocationName.None)
                {
                    toUseE = false;
                    timerToUseE.Start();
                    ChangeLocation();
                    if (currentLocation == LocationName.GamePassed) MessageBox.Show("Игра пройдена!");
                }
                else if ((currentLocation == LocationName.Search || currentLocation == LocationName.Rescue) &&
                    labelToTakeItem.Visible)
                {
                    toUseE = false;
                    timerToUseE.Start();
                    if (currentLocation == LocationName.Search) items.Remove(items[nearItemIndex]);
                    else items[nearItemIndex].IsRescued = true;
                    ++numberOfCollectedItems;
                    if (numberOfCollectedItems == itemsNumber)
                    {
                        if (currentLocation == LocationName.Search) isSearchPassed = true;
                        else isRescuePassed = true;
                        labelForTimeLeft.Visible = false;
                    }
                }
                else if (currentLocation == LocationName.Search && !isAbilityUsed)
                {
                    toUseE = false;
                    timerToUseE.Start();
                    isAbilityUsed = true;
                    var start = new Point(wallace.X / map.TextureWidth, wallace.Y / map.TextureHeight);
                    var queue = new Queue<Point>();
                    queue.Enqueue(start);
                    var visitedPoints = new HashSet<Point>() { start };
                    var pathsFromKeyPartsToWallace = new Dictionary<Point, SinglyLinkedList<Point>>
                    {
                        { start, new SinglyLinkedList<Point>(start) }
                    };

                    FindPaths(queue, map, visitedPoints, pathsFromKeyPartsToWallace);

                    foreach (var item in items)
                        if (pathsFromKeyPartsToWallace.ContainsKey(item.PointOnMap))
                        {
                            MarkTheWay(pathsFromKeyPartsToWallace[item.PointOnMap]);
                            return;
                        }
                }
                return;
            }
            else
            {
                if (isPressedAnyKey) FreeKey();
                return;
            }
        }

        private void MarkTheWay(SinglyLinkedList<Point> pathsFromKeyPartsToWallace)
        {
            var previous = pathsFromKeyPartsToWallace;
            while (previous != null)
            {
                map.MapLayout[previous.Value.X, previous.Value.Y] = 2;
                previous = previous.Previous;
            }
        }

        private void FindPaths(Queue<Point> queue, Map map, HashSet<Point> visitedPoints,
            Dictionary<Point, SinglyLinkedList<Point>> pathsFromKeyPartsToWallace)
        {
            while (queue.Count != 0)
            {
                var point = queue.Dequeue();
                if (point.X < 1 || point.X > map.MapLayout.GetLength(0) - 3 ||
                    point.Y < 1 || point.Y > map.MapLayout.GetLength(1) - 3) continue;

                for (var dy = -1; dy <= 1; ++dy)
                    for (var dx = -1; dx <= 1; ++dx)
                    {
                        //if (dx != 0 && dy != 0) continue; если нужны пути без диагонали - расскоментируй
                        var nextPoint = new Point { X = point.X + dx, Y = point.Y + dy };
                        if (visitedPoints.Contains(nextPoint)) continue;

                        queue.Enqueue(nextPoint);
                        visitedPoints.Add(nextPoint);
                        pathsFromKeyPartsToWallace.Add(nextPoint,
                            new SinglyLinkedList<Point>(nextPoint, pathsFromKeyPartsToWallace[point]));
                    }
            }
        }

        private void FreeKey()
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
            PlayAnimationMovementBot(gr);
            DrawItems(gr);
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
            if (isNear && !items[i - 1].IsRescued)
            {
                labelToTakeItem.Visible = true;
                nearItemIndex = i - 1;
            }
            else labelToTakeItem.Visible = false;
        }

        private void DrawItems(Graphics gr)
        {
            for (var i = 0; i < items.Count; ++i)
            {
                var image = items[i].Image;
                if (items[i].IsRescued) image = new Bitmap(
                    $"{partPathImage}Panda_{items[i].CurrentAnimation}_{items[i].CurrentFrame}{extension}");
                gr.DrawImage(image, items[i].X + cameraOffset.X,
                    items[i].Y + cameraOffset.Y, items[i].Image.Size.Width, items[i].Image.Size.Height);
            }
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
            botImage = new Bitmap($"{partPathImage}Penguin_{direction}_{frameBot}{extension}");
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
                        case 2: // grass with point
                            gr.DrawImage(map.GrassWithPointImage, i * map.TextureWidth + cameraOffset.X,
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
                else if (wallace.X + wallace.Size.Width >map.TextureWidth * (map.MapLayout.GetLength(0) - 1) - labelRange && // right
                    wallace.Y + wallace.Size.Height / 2 > map.TextureHeight * 4 &&
                    wallace.Y + wallace.Size.Height / 2 < map.TextureHeight * 6 &&
                    (isSearchPassed || isRescuePassed))
                    nextLocation = LocationName.Rescue;
                else
                {
                    if (!isAbilityUsed && !isSearchPassed && !labelToTakeItem.Visible)
                        labelForAbility.Visible = true;
                    else labelForAbility.Visible = false;
                    labelToChangeLocation.Visible = false;
                    nextLocation = LocationName.None;
                    return;
                }
                labelForAbility.Visible = false;
                labelToChangeLocation.Visible = true;
            }
            else if (currentLocation == LocationName.Rescue)
            {
                if (!isRescuePassed) labelForTimeLeft.Visible = true;
                if (wallace.Y + wallace.Size.Height / 2 > map.TextureHeight * 4 && // left
                    wallace.Y + wallace.Size.Height / 2 < map.TextureHeight * 6 &&
                    wallace.X < map.TextureWidth + labelRange &&
                    (isRescuePassed || isSearchPassed))
                    nextLocation = LocationName.Search;
                else if (wallace.Y < map.TextureHeight + labelRange && // up
                    wallace.X + wallace.Size.Width / 2 > map.TextureWidth * 4 &&
                    wallace.X + wallace.Size.Width / 2 < map.TextureWidth * 6)
                    nextLocation = LocationName.Initial;
                else if (wallace.X + wallace.Size.Width > map.TextureWidth * (map.MapLayout.GetLength(0) - 1) - labelRange && // right
                    wallace.Y + wallace.Size.Height / 2 > map.TextureHeight * 4 &&
                    wallace.Y + wallace.Size.Height / 2 < map.TextureHeight * 6 &&
                    isRescuePassed)
                    nextLocation = LocationName.GamePassed;
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
                    labelForTimeLeft.Visible = false;
                    timerForSearch.Stop();
                    items.Clear();
                    map.MapLayout = mapLayouts.InitialLayout;
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
                    labelForTimeLeft.Visible = false;
                    timerForSearch.Stop();
                    items.Clear();
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
                    map.MapLayout = mapLayouts.SurvivalLayout;
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
                    labelToTakeItem.Text = "Нажми латинскую клавишу \"E\", чтобы подобрать часть ключа";
                    map.MapLayout = new int[mapLayouts.SearchLayout.GetLength(0), mapLayouts.SearchLayout.GetLength(1)];
                    for (var x = 0; x < mapLayouts.SearchLayout.GetLength(0); ++x)
                        for (var y = 0; y < mapLayouts.SearchLayout.GetLength(1); ++y)
                            map.MapLayout[x, y] = mapLayouts.SearchLayout[x, y];
                    isAbilityUsed = false;
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
                    currentLocation = LocationName.Search;
                    if (!isSearchPassed) InitializeLocation(rnd, "Key_");
                    break;
                case LocationName.Rescue:
                    items.Clear();
                    labelToTakeItem.Text = "Нажмите латинскую клавишу \"E\", чтобы спасти панду";
                    map.MapLayout = mapLayouts.RescueLayout;
                    if (isRescuePassed)
                    {
                        map.MapLayout[map.MapLayout.GetLength(0) - 1, 4] = 0;
                        map.MapLayout[map.MapLayout.GetLength(0) - 1, 5] = 0;
                    }
                    switch (currentLocation)
                    {
                        case LocationName.Initial:
                            previousLocation = LocationName.Initial;
                            wallace.X = map.TextureWidth * 4;
                            wallace.Y = map.TextureHeight + wallace.Size.Height / 2;
                            break;
                        case LocationName.Search:
                            previousLocation = LocationName.Search;
                            wallace.X = map.TextureWidth + wallace.Size.Width / 2;
                            wallace.Y = map.TextureHeight * 4;
                            break;
                    }
                    currentLocation = LocationName.Rescue;
                    cameraOffset = new Point(0, 0);
                    if (!isRescuePassed) InitializeLocation(rnd, "Panda_Right_0");
                    else for (var i = 0; i < itemsNumber; ++i)
                        {
                            var x = pointsToMovePandas[i].X;
                            var y = pointsToMovePandas[i].Y;
                            items.Add(new Item(x, y, new Bitmap($"{partPathImage}Panda_Left_0{extension}"),
                                new Point(x / map.TextureWidth, y / map.TextureHeight)));
                            items[i].IsRescued = true;
                            items[i].CurrentAnimation = AnimationDirection.Left;
                        }
                    break;
                case LocationName.GamePassed:
                    items.Clear();
                    map.MapLayout = mapLayouts.GamePassed;
                    wallace.X = initialWallaceLocation.X;
                    wallace.Y = initialWallaceLocation.Y;
                    cameraOffset = new Point(0, 0);
                    currentLocation = LocationName.GamePassed;
                    labelToChangeLocation.Visible = false;
                    break;
            }
        }

        private void InitializeLocation(Random rnd, string imageName)
        {
            numberOfCollectedItems = 0;
            timeLeft = time;
            labelForTimeLeft.Text = $"Осталось времени: {timeLeft}";
            timerForSearch.Start();
            items.Clear();
            Bitmap image;
            for (var i = 0; i < itemsNumber; ++i)
            {
                if (currentLocation == LocationName.Search)
                    image = new Bitmap($"{partPathImage}{imageName}{i}{extension}");
                else image = new Bitmap($"{partPathImage}{imageName}{extension}");
                var x = rnd.Next(map.TextureWidth + 10, map.TextureWidth * (map.MapLayout.GetLength(0) - 2) - 10);
                var y = rnd.Next(map.TextureHeight + 10, map.TextureHeight * (map.MapLayout.GetLength(1) - 2) - 10);
                if (currentLocation == LocationName.Rescue)
                {
                    if (x % 2 != 0) ++x;
                    if (y % 2 != 0) ++y;
                }
                items.Add(new Item(x, y, image, new Point(x / map.TextureWidth, y / map.TextureHeight)));
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
            var steps = currentLocation == LocationName.Survival ? bots.Count : items.Count;
            for (var i = 0; i < steps; ++i)
            {
                bool isChangedY = false;
                bool isChangedX = false;
                if ((currentLocation == LocationName.Survival && bots[i].X == wallace.X) ||
                    (currentLocation == LocationName.Rescue && items[i].IsRescued && items[i].X == pointsToMovePandas[i].X))
                {
                    if ((currentLocation == LocationName.Survival && bots[i].Y > wallace.Y) ||
                        (currentLocation == LocationName.Rescue && items[i].Y > pointsToMovePandas[i].Y))
                    {
                        if (currentLocation == LocationName.Survival) bots[i].Y -= bots[i].Speed;
                        else items[i].Y -= items[i].Speed;
                    }
                    else if ((currentLocation == LocationName.Survival && bots[i].Y < wallace.Y) ||
                        (currentLocation == LocationName.Rescue && items[i].Y < pointsToMovePandas[i].Y))
                    {
                        if (currentLocation == LocationName.Survival) bots[i].Y += bots[i].Speed;
                        else items[i].Y += items[i].Speed;
                    }

                    isChangedY = true;
                }
                else
                {
                    if ((currentLocation == LocationName.Survival && bots[i].X > wallace.X) ||
                        (currentLocation == LocationName.Rescue && items[i].IsRescued && items[i].X > pointsToMovePandas[i].X))
                    {
                        if (currentLocation == LocationName.Survival)
                        {
                            bots[i].X -= bots[i].Speed;
                            bots[i].CurrentAnimation = AnimationDirection.Left;
                        }
                        else
                        {
                            items[i].X -= items[i].Speed;
                            items[i].CurrentAnimation = AnimationDirection.Left;
                        }
                    }
                    else if ((currentLocation == LocationName.Survival && bots[i].X < wallace.X) ||
                        (currentLocation == LocationName.Rescue && items[i].IsRescued && items[i].X < pointsToMovePandas[i].X))
                    {
                        if (currentLocation == LocationName.Survival)
                        {
                            bots[i].X += bots[i].Speed;
                            bots[i].CurrentAnimation = AnimationDirection.Right;
                        }
                        else
                        {
                            items[i].X += items[i].Speed;
                            items[i].CurrentAnimation = AnimationDirection.Right;
                        }
                    }
                    isChangedX = true;
                }
                if ((currentLocation == LocationName.Survival && bots[i].Y == wallace.Y) ||
                    (currentLocation == LocationName.Rescue && items[i].IsRescued && items[i].Y == pointsToMovePandas[i].Y))
                {

                    if (!isChangedX)
                    {
                        if ((currentLocation == LocationName.Survival && bots[i].X > wallace.X) ||
                            (currentLocation == LocationName.Rescue && items[i].X > pointsToMovePandas[i].X))
                        {
                            if (currentLocation == LocationName.Survival)
                            {
                                bots[i].X -= bots[i].Speed;
                                bots[i].CurrentAnimation = AnimationDirection.Left;
                            }
                            else
                            {
                                items[i].X -= items[i].Speed;
                                items[i].CurrentAnimation = AnimationDirection.Left;
                            }
                        }
                        else if ((currentLocation == LocationName.Survival && bots[i].X < wallace.X) ||
                            (currentLocation == LocationName.Rescue && items[i].X < pointsToMovePandas[i].X))
                        {
                            if (currentLocation == LocationName.Survival)
                            {
                                bots[i].X += bots[i].Speed;
                                bots[i].CurrentAnimation = AnimationDirection.Right;
                            }
                            else
                            {
                                items[i].X += items[i].Speed;
                                items[i].CurrentAnimation = AnimationDirection.Right;
                            }
                        }
                    }
                }
                else
                {
                    if (!isChangedY)
                    {
                        if ((currentLocation == LocationName.Survival && bots[i].Y > wallace.Y) ||
                            (currentLocation == LocationName.Rescue && items[i].IsRescued && items[i].Y > pointsToMovePandas[i].Y))
                        {
                            if (currentLocation == LocationName.Survival) bots[i].Y -= bots[i].Speed;
                            else items[i].Y -= items[i].Speed;
                        }
                            
                        else if ((currentLocation == LocationName.Survival && bots[i].Y < wallace.Y) ||
                            (currentLocation == LocationName.Rescue && items[i].IsRescued && items[i].Y < pointsToMovePandas[i].Y))
                        {
                            if (currentLocation == LocationName.Survival) bots[i].Y += bots[i].Speed;
                            else items[i].Y += items[i].Speed;
                        }
                    }
                }
            }
        }
    }
}
