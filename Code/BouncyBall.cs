using Sandbox;

namespace MightyBrick;

[Title( "Classic Bouncy Ball" ), Icon( "sports_volleyball" )]
public sealed class BouncyBall : Component, Component.IPressable, Component.INetworkSpawn
{
	[Property]
	public Color[] Colors { get; set; }

	[Property]
	public SoundEvent EatSound { get; set; }

	public SphereCollider Collider { get; private set; }
	public SpriteRenderer SpriteRenderer { get; private set; }

	protected override void OnAwake()
	{
		Collider = GetComponent<SphereCollider>();
		SpriteRenderer = GetComponentInChildren<SpriteRenderer>();
	}

	protected override void OnStart()
	{
		if ( IsProxy || !Collider.IsValid() ) 
			return;
		SetRandomScale();
		SetRandomColor();
	}

	protected override void OnUpdate()
	{
		KeepUpright();
	}

	public bool Press( IPressable.Event e )
	{
		Eat();
		return true;
	}

	public void OnNetworkSpawn( Connection _ ) => Network.AssignOwnership( Rpc.Caller );

	private void SetRandomScale()
	{
		WorldScale = Vector3.One * Game.Random.Float( 0.5f, 1.25f );
		WorldPosition += Vector3.Up * Collider.Radius * WorldScale;
	}

	private void SetRandomColor()
	{
		if ( !SpriteRenderer.IsValid() || Colors.Length <= 0 )
			return;
		SpriteRenderer.Color = Colors[Game.Random.Int( Colors.Length - 1 )];
	}

	private void KeepUpright()
	{
		if ( !SpriteRenderer.IsValid() )
			return;
		SpriteRenderer.WorldRotation = Rotation.Identity;
	}

	[Rpc.Broadcast]
	private void Eat()
	{
		if ( EatSound.IsValid() )
			Sound.Play( EatSound, WorldPosition );
		if ( !IsProxy )
			GameObject.Destroy();
	}
}
