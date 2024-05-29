class Blade {
    angle: number;
    damage: number;
    length: number;

    constructor(raw: BladeDto) {
        this.angle = raw.a;
        this.damage = raw.d;
        this.length = raw.l;
    }
}

class Bonus {
    name: string;
    position: number[];

    constructor(raw: BonusDto) {
        this.name = raw.n;
        this.position = raw.p;
    }

    isUseable(player: Player | null) {
        if (!player) return true;

        if (this.name === BonusNames.BladeCount || this.name === BonusNames.BladeCount3) {
            // 刀数量不能超过半径除以8（向上取值），默认半径30，最多3.75->4把刀，减肥时不掉刀
            const maxBladeCount = Math.ceil(player.getSize() / 8);
            return player.blades.length < maxBladeCount;
        }

        if (this.name === BonusNames.BladeLength || this.name === BonusNames.BladeLength20) {
            // 刀长不能超过玩家半径的3倍（但不掉长度），例如：默认刀长30，玩家半径30，最大刀长90
            const maxBladeLength = player.getSize() * 3;
            return player.blades.some(blade => blade.length < maxBladeLength);
        }

        if (this.name === BonusNames.BladeDamage) {
            // 刀伤不能超过半径除以15，默认半径30，最多2伤，减肥时会掉刀伤
            const maxBladeDamage = player.getSize() / 15;
            return player.blades.some(blade => blade.damage < maxBladeDamage);
        }

        if (this.name === BonusNames.BladeSpeed || this.name === BonusNames.BladeSpeed20) {
            // 刀速不能超过玩家半径的1.5倍（但不掉速度），起始10度每秒，半径30，最大45度每秒
            // 前端没有刀速的显示，所以这个判断是暂时做不到的
        }

        return true;
    }
}

class BonusNames {
    public static readonly Health: string = "生命";
    public static readonly Thin: string = "减肥";
    public static readonly Speed: string = "移速+5";
    public static readonly Speed20: string = "移速+20";
    public static readonly BladeCount: string = "刀数";
    public static readonly BladeCount3: string = "刀数+3";
    public static readonly BladeLength: string = "刀长+5";
    public static readonly BladeLength20: string = "刀长+20";
    public static readonly BladeDamage: string = "刀伤";
    public static readonly BladeSpeed: string = "刀速+5";
    public static readonly BladeSpeed20: string = "刀速+20";
    public static readonly Random: string = "随机";
}

class Player {
    userId: number;
    userName: string;
    position: number[];
    destination: number[];
    health: number;
    static defaultSize = 30;
    static minSize = 20;
    blades: Blade[];
    score: number;

    constructor(raw: PlayerDto) {
        this.userId = raw.u;
        this.userName = raw.n;
        this.position = raw.p;
        this.destination = raw.d;
        this.health = raw.h;
        this.blades = raw.b.map(bladeRaw => new Blade(bladeRaw));
        this.score = raw.s;
    }

    getSize() {
        const suggestedSize = Player.minSize + this.health;
        return suggestedSize < 0 ? 0 : suggestedSize;
    }

    // 平衡性设计：如果刀比较少，对刀时不减少伤害，此时刀的颜色为金色
    isGoldBlade(blade: Blade) {
        return this.blades.length <= 2 && blade.damage >= 2;
    }
}
// Define the structure for player data
type PlayerDto = {
    u: number;  // Unique user ID
    n: string;  // Username
    s: number;  // Score of the player
    p: number[];  // Current position in the game world, usually an array of x, y, z coordinates
    d: number[];  // Destination position in the game world, similar to 'p'
    h: number;  // Health level of the player
    b: BladeDto[];  // Array of blades the player has
};

// Define the structure for pickable bonuses in the game
type BonusDto = {
    n: string;  // Name of the bonus
    p: number[];  // Position where the bonus is located, typically x, y, z coordinates
};

// Define the structure for a blade, part of a player's weaponry
type BladeDto = {
    a: number;  // Angle at which the blade is being held or used
    l: number;  // Length of the blade
    d: number;  // Damage potential of the blade
};

type PushStateDto = {
    i: number; // frame index
    p: PlayerDto[]; // Array of player data
    b: BonusDto[]; // Array of pickable bonus data
    d: PlayerDto[]; // Array of dead player data
};

declare var initState: PushStateDto;