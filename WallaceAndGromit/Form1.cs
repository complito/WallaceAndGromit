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
    public partial class Form1 : Form
    {
        private enum AnimationDirection
        {
            Left,
            Right,
            Up,
            Down,
            None
        }
        private AnimationDirection currentAnimation = AnimationDirection.None;
        private AnimationDirection previousAnimation = AnimationDirection.Right;
        private int currentFrame = 0;
        private bool isPressedAnyKey = false;
        private Image wallaceImage;
        private string partPathImage = "D:\\УрФУ\\ЯТП\\WallaceAndGromit\\WallaceAndGromit\\images\\Wallace";
        private string extension = ".png";
        private PictureBox pictureBox = new PictureBox
        {
            Location = new Point(50, 50),
            SizeMode = PictureBoxSizeMode.StretchImage,
            Size = new Size(192, 304)
        };
        private PictureBox pictureBox2 = new PictureBox
        {
            Location = new Point(300, 50),
            SizeMode = PictureBoxSizeMode.StretchImage,
            Size = new Size(192, 304)
        };
        private Timer timerAnimation = new Timer
        {
            Interval = 100
        };
        private Timer timerMovement = new Timer
        {
            Interval = 10
        };
        public Form1()
        {
            InitializeComponent();
            wallaceImage = new Bitmap($"{partPathImage}_Right_{currentFrame}{extension}");
            pictureBox.Image = wallaceImage;
            pictureBox2.Image = wallaceImage;
            Controls.Add(pictureBox);
            Controls.Add(pictureBox2);
            timerAnimation.Tick += new EventHandler(UpdateAnimation);
            timerAnimation.Start();
            timerMovement.Tick += new EventHandler(UpdateMovement);
            timerMovement.Start();
            KeyDown += new KeyEventHandler(Keybord);
            KeyUp += new KeyEventHandler(FreeKey);
        }

        private void FreeKey(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode.ToString())
            {
                case "A":
                    pictureBox.Image = new Bitmap($"{partPathImage}_Left_0{extension}");
                    break;
                case "D":
                    pictureBox.Image = new Bitmap($"{partPathImage}_Right_0{extension}");
                    break;
                case "W":
                    if (previousAnimation == AnimationDirection.Left)
                        pictureBox.Image = new Bitmap($"{partPathImage}_Left_0{extension}");
                    else if (previousAnimation == AnimationDirection.Right)
                        pictureBox.Image = new Bitmap($"{partPathImage}_Right_0{extension}");
                    break;
                case "S":
                    if (previousAnimation == AnimationDirection.Left)
                        pictureBox.Image = new Bitmap($"{partPathImage}_Left_0{extension}");
                    else if (previousAnimation == AnimationDirection.Right)
                        pictureBox.Image = new Bitmap($"{partPathImage}_Right_0{extension}");
                    break;
            }
            isPressedAnyKey = false;
            if (currentAnimation == AnimationDirection.Left || currentAnimation == AnimationDirection.Right)
                previousAnimation = currentAnimation;
            currentAnimation = AnimationDirection.None;
            currentFrame = 0;
        }

        private void UpdateAnimation(object sender, EventArgs e)
        {
            if (isPressedAnyKey) PlayAnimationMovement();
            if (currentFrame == 3) currentFrame = -1;
            ++currentFrame;
        }

        private void UpdateMovement(object sender, EventArgs e)
        {
            switch (currentAnimation)
            {
                case AnimationDirection.Left:
                    pictureBox.Location = new Point(pictureBox.Location.X - 2, pictureBox.Location.Y);
                    break;
                case AnimationDirection.Right:
                    pictureBox.Location = new Point(pictureBox.Location.X + 2, pictureBox.Location.Y);
                    break;
                case AnimationDirection.Up:
                    pictureBox.Location = new Point(pictureBox.Location.X, pictureBox.Location.Y - 2);
                    break;
                case AnimationDirection.Down:
                    pictureBox.Location = new Point(pictureBox.Location.X, pictureBox.Location.Y + 2);
                    break;
            }
        }

        private void PlayAnimationMovement()
        {
            switch (currentAnimation)
            {
                case AnimationDirection.Left:
                    pictureBox.Image = new Bitmap($"{partPathImage}_Left_{currentFrame}{extension}");
                    break;
                case AnimationDirection.Right:
                    pictureBox.Image = new Bitmap($"{partPathImage}_Right_{currentFrame}{extension}");
                    break;
                case AnimationDirection.Up:
                    if (previousAnimation == AnimationDirection.Left)
                        pictureBox.Image = new Bitmap($"{partPathImage}_Left_{currentFrame}{extension}");
                    else if (previousAnimation == AnimationDirection.Right)
                        pictureBox.Image = new Bitmap($"{partPathImage}_Right_{currentFrame}{extension}");
                    break;
                case AnimationDirection.Down:
                    if (previousAnimation == AnimationDirection.Left)
                        pictureBox.Image = new Bitmap($"{partPathImage}_Left_{currentFrame}{extension}");
                    else if (previousAnimation == AnimationDirection.Right)
                        pictureBox.Image = new Bitmap($"{partPathImage}_Right_{currentFrame}{extension}");
                    break;
            }
        }

        private void Keybord(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode.ToString())
            {
                case "A":
                    currentAnimation = AnimationDirection.Left;
                    break;
                case "D":
                    currentAnimation = AnimationDirection.Right;
                    break;
                case "W":
                    currentAnimation = AnimationDirection.Up;
                    break;
                case "S":
                    currentAnimation = AnimationDirection.Down;
                    break;
            }
            isPressedAnyKey = true;
        }
    }
}
