using CustomExtensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class CardConstant
{
	public static List<CardConstant> List = new List<CardConstant> { };

	public static void LoadFromBytes(byte[][] bytes)
	{
		List.Clear();

		for (ushort i = 0; i < Cards.TotalCardCount; i++)
		{
			List.Add(new CardConstant(i, bytes[i]));
		}
	}

	public static List<CardConstant> Monsters
	{
		get
		{
			CardColorType[] monsterTypes = { CardColorType.NormalMonster, CardColorType.EffectMonster };
			return List.FindAll(constant => monsterTypes.Contains(constant.CardColor)).ToList<CardConstant>();
		}
	}

	public static byte[] AllBytes
	{
		get
		{
			return List.SelectMany(c => c.Bytes).ToArray();
		}
	}

	public CardColorType CardColor { get; set; }

	const int maxAttackDefense = 8191;
	byte[] bytes;
	byte kind;
	CardKind cardKind;
	byte kindOfs;
	BitArray levelAttribute;
	byte level;
	CardAttribute attribute;
	ushort effectId;
	ushort xaxId;
	BitArray apWithFlags;
	ushort attack;
	bool hasAlternateArt;
	bool isSlotRare;
	bool appearsInSlotReels;
	BitArray dpWithFlags;
	ushort defense;
	bool hasImage;
	bool passwordWorks;
	bool appearsInReincarnation;
	byte[] passwordArray;

	public CardConstant(ushort cardIndex, byte[] bytes)
	{
		this.Index = cardIndex;
		this.Name = Cards.GetNameByIndex(cardIndex);
		this.bytes = bytes;
		this.kind = bytes[0];
		this.cardKind = new CardKind(this.kind);
		this.kindOfs = bytes[1];
		this.levelAttribute = new BitArray(new byte[] { bytes[2] });
		this.level = bytes[2].splitByte()[1];
		this.attribute = new CardAttribute(bytes[2].splitByte()[0]);
		this.DeckCost = bytes[3];
		this.effectId = BitConverter.ToUInt16(new byte[] { bytes[4], bytes[5] }, 0);
		this.xaxId = BitConverter.ToUInt16(new byte[] { bytes[6], bytes[7] }, 0);

		this.apWithFlags = new BitArray(new byte[] { bytes[8], bytes[9] });
		this.attack = CardConstant.GetAttackOrDefense(new byte[] { bytes[8], bytes[9] });
		this.hasImage = apWithFlags[apWithFlags.Length - 3];
		this.passwordWorks = apWithFlags[apWithFlags.Length - 2];
		this.appearsInReincarnation = apWithFlags[apWithFlags.Length - 1];

		this.dpWithFlags = new BitArray(new byte[] { bytes[10], bytes[11] });
		this.defense = CardConstant.GetAttackOrDefense(new byte[] { bytes[10], bytes[11] });
		this.appearsInSlotReels = dpWithFlags[dpWithFlags.Length - 3];
		this.isSlotRare = dpWithFlags[dpWithFlags.Length - 2];

		this.hasAlternateArt = dpWithFlags[dpWithFlags.Length - 1];
		this.passwordArray = new byte[] { bytes[12], bytes[13], bytes[14], bytes[15], bytes[16], bytes[17], bytes[18], bytes[19] };
		this.setCardColor();
	}

	private void setCardColor()
  {
		if (this.EffectId == 65535)
		{
			this.CardColor = CardColorType.NormalMonster;
		}
		else if (this.Kind.Id < 32)
		{
			this.CardColor = CardColorType.EffectMonster;
		}
		else if (this.Kind.Id == 96 || this.Kind.Id == 128)
		{
			this.CardColor = CardColorType.Trap;
		}
		else if (this.Kind.Id == 160)
		{
			this.CardColor = CardColorType.Ritual;
		}
		else
		{
			this.CardColor = CardColorType.Magic;
		}
	}

	public static ushort GetAttackOrDefense(byte[] bytes)
	{
		BitArray bitArray = new BitArray(bytes);
		bitArray[bitArray.Length - 1] = false;
		bitArray[bitArray.Length - 2] = false;
		bitArray[bitArray.Length - 3] = false;
		byte[] newBytes = new byte[2];
		bitArray.CopyTo(newBytes, 0);
		return BitConverter.ToUInt16(newBytes, 0);
	}

	private byte[] calculateByteSequence()
	{
		byte[] newByteArray = new byte[20];
		newByteArray[0] = this.kind;
		newByteArray[1] = this.kindOfs;
		newByteArray[2] = this.levelAttribute.toByte();
		newByteArray[3] = this.DeckCost;
		newByteArray[4] = BitConverter.GetBytes(this.effectId)[0];
		newByteArray[5] = BitConverter.GetBytes(this.effectId)[1];
		newByteArray[6] = BitConverter.GetBytes(this.xaxId)[0];
		newByteArray[7] = BitConverter.GetBytes(this.xaxId)[1];
		byte[] apWithFlagsByteArray = new byte[2];
		this.apWithFlags.CopyTo(apWithFlagsByteArray, 0);
		newByteArray[8] = apWithFlagsByteArray[0];
		newByteArray[9] = apWithFlagsByteArray[1];
		byte[] dpWithFlagsByteArray = new byte[2];
		this.dpWithFlags.CopyTo(dpWithFlagsByteArray, 0);
		newByteArray[10] = dpWithFlagsByteArray[0];
		newByteArray[11] = dpWithFlagsByteArray[1];
		this.passwordArray.CopyTo(newByteArray, 12);

		return newByteArray;
	}

	public byte[] Bytes
	{
		get
		{
			return this.calculateByteSequence();
		}
	}

	public ushort Index { get; }

	public string Name { get; }

	public byte DeckCost { get; set; }

	public CardKind Kind
  {
    get
    {
			return this.cardKind;
    }
  }

	public string Type
  {
    get
    {
			return this.cardKind.Name;
    }
  }

	public string KindName
  {
    get
    {
			return this.cardKind.Name;
    }
  }

	public ushort EffectId
  {
    get
    {
			return this.effectId;
    }
  }

	public byte Level
  {
    get
    {
		  return this.level;
    }

		set
    {
			this.level = value;
			this.levelAttribute.setHalfByte(value, 4);
    }
  }

	public ushort XaxId
  {
    get
    {
			return this.xaxId;
    }
  }

	public string AttributeName
  {
    get
    {
			if (this.cardKind.isMonster())
      {
				return this.attribute.Name;
      }

			return "";
    }
  }

	public ushort Attack
  {
    get
    {
			return this.attack;
    }
		set
    {
			this.attack = this.roundAttackDefense(value);
			byte[] attackBytes = BitConverter.GetBytes(this.attack);
			BitArray attackBitArray = new BitArray(attackBytes);
			attackBitArray.copyRangeTo(new int[] { 0, 12 }, ref this.apWithFlags, 0);
    }
  }

	public ushort Defense
  {
    get
    {
			return this.defense;
    }
		set
    {
			this.defense = this.roundAttackDefense(value);
			byte[] defenseBytes = BitConverter.GetBytes(this.defense);
			BitArray defenseBitArray= new BitArray(defenseBytes);
			defenseBitArray.copyRangeTo(new int[] { 0, 12 }, ref this.dpWithFlags, 0);
		}
  }

	private ushort roundAttackDefense(ushort value)
  {
		if (value < 0)
		{
			value = 0;
		}
		else if (value > CardConstant.maxAttackDefense)
		{
			value = CardConstant.maxAttackDefense;
		}

		return value;
	}

	public bool IsSlotRare
  {
    get
    {
			return this.isSlotRare;
    }

    set
    {
			this.isSlotRare = value;
			this.dpWithFlags[dpWithFlags.Length - 2] = this.isSlotRare;
		}
  }

	public bool AppearsInSlotReels
  {
    get
    {
			return this.appearsInSlotReels;
    }

    set
    {
			this.appearsInSlotReels = value;
			this.dpWithFlags[this.dpWithFlags.Length - 3] = this.appearsInSlotReels;
    }
  }

	public bool AppearsInReincarnation
  {
    get
    {
			return this.appearsInReincarnation;
    }

    set
    {
			this.appearsInReincarnation = value;
			this.apWithFlags[this.apWithFlags.Length - 1] = this.appearsInReincarnation;
		}
  }

	public bool PasswordWorks
  {
    get
    {
			return this.passwordWorks;
    }

    set
    {
			this.passwordWorks = value;
			this.apWithFlags[apWithFlags.Length - 2] = this.passwordWorks;
    }
  }

	public bool HasImage
  {
    get
    {
			return this.hasImage;
    }
  }

	public bool HasAlternateArt
  {
    get
    {
			return this.hasAlternateArt;
    }
  }

	public override string ToString()
  {
		return Name;
  }
}