using Sandbox;
using System.Linq;

namespace MightyBrick;

[Title( "Classic Bouncy Ball" ), Icon( "sports_volleyball" )]
public sealed class BouncyBall : Component, Component.INetworkSpawn, Component.IPressable
{
	[Property]
	public int HealAmount { get; set; } = 10;

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
		SetRandomScale();
		SetRandomColor();
	}

	protected override void OnUpdate()
	{
		KeepUpright();
	}

	public void OnNetworkSpawn( Connection connection )
	{
		Network.AssignOwnership( Rpc.Caller );
	}

	public bool Press( IPressable.Event e )
	{
		Eat( e.Source.GameObject );
		return true;
	}

	private void SetRandomScale()
	{
		if ( IsProxy || !Collider.IsValid() )
			return;
		WorldScale = Vector3.One * Game.Random.Float( 0.5f, 1.25f );
		WorldPosition += Vector3.Up * Collider.Radius * WorldScale;
	}

	private void SetRandomColor()
	{
		if ( IsProxy || Colors.Length <= 0 || !SpriteRenderer.IsValid() )
			return;
		Color color = Colors[Game.Random.Int( Colors.Length - 1 )];
		SetColor( color );
	}

	[Rpc.Broadcast( NetFlags.OwnerOnly )]
	private void SetColor( Color color )
	{
		if ( Colors.Length <= 0 || !SpriteRenderer.IsValid() )
			return;
		SpriteRenderer.Color = color;
	}

	private void KeepUpright()
	{
		if ( !SpriteRenderer.IsValid() )
			return;
		SpriteRenderer.WorldRotation = Rotation.Identity;
	}

	[Rpc.Broadcast]
	private void Eat( GameObject targetPlayer )
	{
		var player = targetPlayer.Components.GetAll().FirstOrDefault( c => c.GetType().Name == "Player" );

		if ( player is Component comp && comp.IsValid() )
		{
			var playerType = TypeLibrary.GetType( player.GetType() );
			var healthProp = playerType.GetProperty( "Health" );

			if ( healthProp != null )
			{
				float health = (float)healthProp.GetValue( player );
				healthProp.SetValue( player, health + HealAmount );
			}
		}

		if ( EatSound.IsValid() )
			Sound.Play( EatSound, WorldPosition );

		if ( !IsProxy )
			GameObject.Destroy();
	}
}
