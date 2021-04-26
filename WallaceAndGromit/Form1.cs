using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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

    public partial class Form1 : Form
    {
        private bool isPressedAnyKey = false;
        private bool toUpdateAnimation = false;
        private Player wallace;
        private Map map;
        private Label label = new Label
        {
            Size = new Size(150, 20),
            Location = new Point(50, 50),
            Text = "Press E to change location",
            Visible = false
        };
        private string partPathImage = "D:\\УрФУ\\ЯТП\\WallaceAndGromit\\WallaceAndGromit\\images\\";
        private string extension = ".png";
        private Timer timerAnimation = new Timer { Interval = 100 };
        private Timer timerMovement = new Timer { Interval = 10 };

        public Form1()
        {
            InitializeComponent();
            Controls.Add(label);
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
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
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

            KeyDown += new KeyEventHandler(Keybord);
            KeyUp += new KeyEventHandler(FreeKey);
            Paint += new PaintEventHandler(OnPaint);
        }

        private void UpdateAnimation(object sender, EventArgs e)
        {
            toUpdateAnimation = true;
        }

        private void UpdateMovement(object sender, EventArgs e)
        {
            ChangeLabelVisible();
            switch (wallace.currentAnimation)
            {
                case AnimationDirection.Left:
                    if (wallace.x > 64) wallace.Left();
                    break;
                case AnimationDirection.Right:
                    if (wallace.x + wallace.size.Width < 1216) wallace.Right();
                    break;
                case AnimationDirection.Up:
                    if (wallace.y > 64) wallace.Up();
                    break;
                case AnimationDirection.Down:
                    if (wallace.y + wallace.size.Height < 639) wallace.Down();
                    break;
                default: return;
            }
            Invalidate();
        }

        private void Keybord(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode.ToString())
            {
                case "A":
                    wallace.currentAnimation = AnimationDirection.Left;
                    break;
                case "D":
                    wallace.currentAnimation = AnimationDirection.Right;
                    break;
                case "W":
                    wallace.currentAnimation = AnimationDirection.Up;
                    break;
                case "S":
                    wallace.currentAnimation = AnimationDirection.Down;
                    break;
                default:
                    return;
            }
            isPressedAnyKey = true;
        }

        private void FreeKey(object sender, KeyEventArgs e)
        {
            isPressedAnyKey = false;
            wallace.currentFrame = 0;
            if (wallace.currentAnimation == AnimationDirection.Left
                || wallace.currentAnimation == AnimationDirection.Right)
                wallace.previousAnimation = wallace.currentAnimation;
            wallace.currentAnimation = AnimationDirection.None;
            Invalidate();
        }

        private void OnPaint(object sender, PaintEventArgs e)
        {
            Graphics gr = e.Graphics;
            CreateMap(gr);
            PlayAnimationMovement(gr);
        }

        private void PlayAnimationMovement(Graphics gr)
        {
            if (isPressedAnyKey)
            {
                if (toUpdateAnimation)
                {
                    if (wallace.currentFrame == 3) wallace.currentFrame = -1;
                    ++wallace.currentFrame;
                    toUpdateAnimation = false;
                }
                switch (wallace.currentAnimation)
                {
                    case AnimationDirection.Left:
                        DrawImage(gr, "Left");
                        break;
                    case AnimationDirection.Right:
                        DrawImage(gr, "Right");
                        break;
                    case AnimationDirection.Up:
                        DrawLeftOrRight(gr);
                        break;
                    case AnimationDirection.Down:
                        DrawLeftOrRight(gr);
                        break;
                }
            }
            else DrawLeftOrRight(gr);
        }

        private void DrawLeftOrRight(Graphics gr)
        {
            if (wallace.previousAnimation == AnimationDirection.Left) DrawImage(gr, "Left");
            else DrawImage(gr, "Right");
        }

        private void DrawImage(Graphics gr, string direction)
        {
            var wallaceImage = new Bitmap($"{partPathImage}Wallace_{direction}_{wallace.currentFrame}{extension}");
            gr.DrawImage(wallaceImage, wallace.x, wallace.y, wallace.size.Width, wallace.size.Height);
        }

        private void CreateMap(Graphics gr)
        {
            for (var i = 0; i < map.map.GetLength(0); ++i)
                for (var j = 0; j < map.map.GetLength(1); ++j)
                    switch (map.map[i, j])
                    {
                        case 0: // grass
                            gr.DrawImage(map.grassImage, i * map.textureWidth, j * map.textureWidth,
                                map.textureWidth, map.textureHeight);
                            break;
                        case 1: // wall
                            gr.DrawImage(map.wallImage, i * map.textureWidth, j * map.textureWidth,
                                map.textureWidth, map.textureHeight);
                            break;
                    }
        }

        private void ChangeLabelVisible()
        {
            if ((wallace.y + wallace.size.Height / 2 > 64 * 4 && // left
                wallace.y + wallace.size.Height / 2 < 64 * 6 &&
                wallace.x < 64 + 50) ||
                (wallace.y + wallace.size.Height / 2 > 64 * 4 && // right
                wallace.y + wallace.size.Height / 2 < 64 * 6 &&
                wallace.x + wallace.size.Width > 1280 - 64 - 50) ||
                (wallace.y < 64 + 50 && // up
                wallace.x + wallace.size.Width / 2 > 64 * 9 &&
                wallace.x + wallace.size.Width / 2 < 64 * 11) ||
                (wallace.y + wallace.size.Height > 639 - 50 && // down
                wallace.x + wallace.size.Width / 2 > 64 * 9 &&
                wallace.x + wallace.size.Width / 2 < 64 * 11))
                label.Visible = true;
            else label.Visible = false;
        }
    }
}
