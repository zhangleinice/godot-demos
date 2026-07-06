using Godot;

public partial class Player : Area2D
{

	[Signal]
	public delegate void HitEventHandler();

	[Export]
	public int Speed { get; set; } = 400;

	// Vector2 二维向量
	public Vector2 ScreenSize;

	// 执行一次，初始化，获取节点、初始化变量等
	public override void _Ready()
	{
		ScreenSize = GetViewportRect().Size;
	}

	// delta：Godot 每帧自动传入，表示距离上一帧经过的时间（单位：秒）
	// 只要游戏在运行，并且这个 Player 节点已经进入场景树，_Process() 就会一直不停地执行。
	public override void _Process(double delta)
	{
		var velocity = Vector2.Zero;

		// 按键只是代表移动方向
		if (Input.IsActionPressed("move_right"))
			velocity.X += 1;

		if (Input.IsActionPressed("move_left"))
			velocity.X -= 1;

		if (Input.IsActionPressed("move_down"))
			velocity.Y += 1;

		if (Input.IsActionPressed("move_up"))
			velocity.Y -= 1;

		var animatedSprite2D = GetNode<AnimatedSprite2D>("AnimatedSprite2D");

		// 如果玩家有输入方向
		if (velocity.Length() > 0)
		{
			// 速度向量 = 方向向量 * 速度
			velocity = velocity.Normalized() * Speed;
			animatedSprite2D.Play();
		}
		else
		{
			animatedSprite2D.Stop();
		}

		// 移动更新位置
		Position += velocity * (float)delta;

		// Mathf.Clamp(value, min, max) 把一个值限制在指定范围内
		// 保证位置在屏幕内
		Position = new Vector2(
			x: Mathf.Clamp(Position.X, 0, ScreenSize.X),
			y: Mathf.Clamp(Position.Y, 0, ScreenSize.Y)
		);

		// 如果 X 方向有速度
		if (velocity.X != 0)
		{
			animatedSprite2D.Animation = "walk";
			// 上下不翻转
			animatedSprite2D.FlipV = false;
			// 图片默认向右，向左就需要翻转
			animatedSprite2D.FlipH = velocity.X < 0;
		}
		else if (velocity.Y != 0)
		{
			animatedSprite2D.Animation = "up";
			animatedSprite2D.FlipV = velocity.Y > 0;
		}

	}

	// 连接 body_entered 信号， 玩家与敌人发生碰撞时自动调用
	private void OnBodyEntered(Node2D body)
	{
		// 玩家被撞后隐藏
		Hide(); // Player disappears after being hit.
		// 发射 Hit 信号
		EmitSignal(SignalName.Hit);
		// CollisionShape2D节点，等这一帧结束以后再改。
		// Must be deferred as we can't change physics properties on a physics callback.
		GetNode<CollisionShape2D>("CollisionShape2D").SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
	}

	public void Start(Vector2 position)
	{
		// 设置初始位置
		Position = position;
		// 显示玩家
		Show();
		// 重新开启碰撞
		GetNode<CollisionShape2D>("CollisionShape2D").Disabled = false;
	}
}
