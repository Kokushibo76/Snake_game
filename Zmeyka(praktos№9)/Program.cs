using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SnakeGame
{
    enum Direction
    {
        UP,
        DOWN,
        LEFT,
        RIGHT
    }

    class SnakeGameLauncher
    {
        static void Main(string[] args)
        {
            SnakeGame game = new SnakeGame();
            game.Start();
        }
    }

    public class Gamewalls
    {
        private readonly char _wallSymbol;
        private readonly List<Point> _wallSections;

        public Gamewalls(int maxX, int maxY, char symbol)
        {
            _wallSymbol = symbol;
            _wallSections = new List<Point>();
            CreateWalls(maxX, maxY);
        }

        private void CreateWalls(int width, int height)
        {
            for (int x = 0; x < width; x++)
            {
                AddWallSection(new Point(x, 0, _wallSymbol));
                AddWallSection(new Point(x, height - 1, _wallSymbol));
            }

            for (int y = 0; y < height; y++)
            {
                AddWallSection(new Point(0, y, _wallSymbol));
                AddWallSection(new Point(width - 1, y, _wallSymbol));
            }
        }

        private void AddWallSection(Point point)
        {
            point.Draw();
            _wallSections.Add(point);
        }

        public bool dodge(Point point)
        {
            return _wallSections.Any(wallSection => wallSection.Equals(point));
        }
    }

    public class Point
    {
        public int X { get; init; }
        public int Y { get; init; }
        public char Symbol { get; init; }

        public Point(int x, int y, char symbol = ' ')
        {
            X = x;
            Y = y;
            Symbol = symbol;
        }

        public void Draw()
        {
            Console.SetCursorPosition(X, Y);
            Console.Write(Symbol);
        }

        public void Draw(char symbol)
        {
            Console.SetCursorPosition(X, Y);
            Console.Write(symbol);
        }

        public override bool Equals(object obj)
        {
            return obj is Point other && X == other.X && Y == other.Y;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }
    }

    class PlayerSnake
    {
        private readonly List<Point> _snake;
        private Direction _direction;
        private Point _tail;
        private Point _head;

        public PlayerSnake(int x, int y, int length)
        {
            _direction = Direction.RIGHT;
            _snake = new List<Point>();

            for (int i = x - length; i < x; i++)
            {
                Point point = new Point(i, y, 'O');
                _snake.Add(point);
                point.Draw();
            }
        }

        public Point GetNextPoint()
        {
            Point head = _snake.Last();
            Point nextPoint = head;

            switch (_direction)
            {
                case Direction.UP:
                    nextPoint = new Point(head.X, head.Y - 1, 'O');
                    break;
                case Direction.DOWN:
                    nextPoint = new Point(head.X, head.Y + 1, 'O');
                    break;
                case Direction.LEFT:
                    nextPoint = new Point(head.X - 1, head.Y, 'O');
                    break;
                case Direction.RIGHT:
                    nextPoint = new Point(head.X + 1, head.Y, 'O');
                    break;
            }
            return nextPoint;
        }

        public void ChangeDirection(ConsoleKey key)
        {
            _direction = key switch
            {
                ConsoleKey.UpArrow when _direction != Direction.DOWN => Direction.UP,
                ConsoleKey.DownArrow when _direction != Direction.UP => Direction.DOWN,
                ConsoleKey.LeftArrow when _direction != Direction.RIGHT => Direction.LEFT,
                ConsoleKey.RightArrow when _direction != Direction.LEFT => Direction.RIGHT,
                _ => _direction
            };
        }

        public Point GetHead() => _snake.Last();

        public void Move()
        {
            _head = GetNextPoint();
            _snake.Add(_head);
            _tail = _snake.First();
            _snake.Remove(_tail);
            _tail.Draw(' ');
            _head.Draw();
        }

        public bool Eat(Point food)
        {
            _head = GetNextPoint();
            if (_head.Equals(food))
            {
                food.Draw('O');
                _snake.Add(food);
                return true;
            }
            return false;
        }

        public bool dodge()
        {
            return _snake.GetRange(0, _snake.Count - 1).Any(p => p.Equals(GetHead()));
        }
    }

    class SnakeGame
    {
        private const int XWall = 80;
        private const int YWall = 25;
        private Gamewalls _walls;
        private PlayerSnake _snake;
        private points _points;
        private Timer _timer;

        public void Start()
        {
            Console.SetWindowSize(XWall + 1, YWall + 1);
            Console.SetBufferSize(XWall + 1, YWall + 1);
            Console.CursorVisible = false;

            _walls = new Gamewalls(XWall, YWall, '/');
            _snake = new PlayerSnake(XWall / 2, YWall / 2, 3);
            _points = new points(XWall, YWall, '@');
            _points.CreateFood();

            _timer = new Timer(Loop, null, 0, 200);

            while (!_snake.dodge() && !_walls.dodge(_snake.GetHead()))
            {
                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo key = Console.ReadKey(true);
                    _snake.ChangeDirection(key.Key);
                }
            }
            _timer.Dispose();
            GameOver();
        }

        private void Loop(object state)
        {
            if (_snake.Eat(_points.Food))
            {
                _points.CreateFood();
            }
            else
            {
                _snake.Move();
            }
            if (_walls.dodge(_snake.GetHead()) || _snake.dodge())
            {
                _timer.Change(Timeout.Infinite, Timeout.Infinite);
            }
        }

        private void GameOver()
        {
            Console.SetCursorPosition(XWall / 2 - 5, YWall / 2);
            Console.Write("ИГРА ОКОНЧЕНА");
            Console.ReadKey();
            _timer.Dispose();
            Environment.Exit(0);
        }
    }

    class points
    {
        private readonly int _maxX;
        private readonly int _maxY;
        private readonly char _foodSymbol;
        private readonly Random _random = new Random();
        public Point Food { get; private set; }

        public points(int maxX, int maxY, char foodSymbol)
        {
            _maxX = maxX;
            _maxY = maxY;
            _foodSymbol = foodSymbol;
        }

        public void CreateFood()
        {
            int x = _random.Next(1, _maxX - 1);
            int y = _random.Next(1, _maxY - 1);
            Food = new Point(x, y, _foodSymbol);
            Food.Draw();
        }
    }
}