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
    type: BonusType;
    position: number[];

    constructor(raw: BonusDto) {
        this.type = raw.t;
        this.position = raw.p;
    }

    get name() {
        return BonusNames.toDisplayString(this.type);
    }

    isUseable(player: Player | null) {
        if (!player) return true;

        if (this.type === BonusType.BladeCount || this.type === BonusType.BladeCount3) {
            // 刀数量不能超过半径除以8（向上取值），默认半径30，最多3.75->4把刀，减肥时不掉刀
            const maxBladeCount = Math.ceil(player.size / 8);
            return player.blades.length < maxBladeCount;
        }

        if (this.type === BonusType.BladeLength || this.type === BonusType.BladeLength20) {
            // 在玩家半径为20时，刀长倍率为6，半径为200时，刀长倍率为3，非线性递减
            const bladeLengthToPlayerSize = 4.5 * Math.exp(-0.02 * player.size) + 2.9375;
            const maxBladeLength = player.size * bladeLengthToPlayerSize;
            return player.blades.some(blade => blade.length < maxBladeLength);
        }

        if (this.type === BonusType.BladeDamage) {
            // 刀伤不能超过半径除以12，默认半径30，最多2.5伤，减肥时会掉刀伤
            const maxBladeDamage = player.size / 12;
            return player.blades.some(blade => blade.damage < maxBladeDamage);
        }

        if (this.type === BonusType.BladeSpeed || this.type === BonusType.BladeSpeed20) {
            // 刀速不能超过60（初始值10）
            const maxBladeSpeed = 60;
            return player.bladeRotationSpeed < maxBladeSpeed;
        }

        return true;
    }
}

class UserNameCache {
    names: { [userId: number]: string } = {};
    ongoingRequests: { [userId: number]: Promise<any> | undefined } = {};

    getUserNameASAP(userId: number) {
        if (this.names[userId]) return this.names[userId];
        if (!this.ongoingRequests[userId]) {
            this.ongoingRequests[userId] = this.loadUserName(userId).then(name => {
                this.names[userId] = name;
                this.ongoingRequests[userId] = undefined;
            });
        }
        
        return userId.toString();
    }

    private async loadUserName(userId: number) {
        // GET: /user/{userId}/name, need token
        const response = await fetch(`/user/${userId}/name`, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': 'Bearer ' + await ensureToken()
            }
        });

        return await response.text();
    }
}
const userNameCache = new UserNameCache();

class Player {
    static defaultSize = 30;
    static minSize = 20;

    userId: number;
    position: number[];
    destination: number[];
    health: number;
    blades: Blade[];
    score: number;
    bladeRotationSpeed: number;

    constructor(raw: PlayerDto) {
        this.userId = raw.u;
        this.position = raw.p;
        this.destination = raw.d;
        this.health = raw.h;
        this.blades = raw.b.map(bladeRaw => new Blade(bladeRaw));
        this.score = raw.s;
        this.bladeRotationSpeed = raw.z;
    }

    get userName(): string {
        return userNameCache.getUserNameASAP(this.userId);
    }

    get size() {
        const suggestedSize = Math.min(Player.minSize + this.health, 250);
        return Math.max(suggestedSize, 1);
    }

    // 平衡性设计：如果刀比较少，对刀时不减少伤害，此时刀的颜色为金色
    isGoldBlade(blade: Blade) {
        return this.blades.length <= 2 && blade.damage >= 2;
    }
}

// Define the structure for player data
type PlayerDto = {
    u: number;  // Unique user ID
    s: number;  // Score of the player
    p: number[];  // Current position in the game world, usually an array of x, y, z coordinates
    d: number[];  // Destination position in the game world, similar to 'p'
    h: number;  // Health level of the player
    z: number; // blade rotation speed degree in second
    b: BladeDto[];  // Array of blades the player has
};

enum BonusType {
    Health,
    Thin,
    Speed,
    Speed20,
    BladeCount,
    BladeCount3,
    BladeLength,
    BladeLength20,
    BladeDamage,
    BladeSpeed,
    BladeSpeed20,
    Random
}

class BonusNames {
    private static bonusTypeToNameMap: Map<BonusType, string> = new Map<BonusType, string>([
        [BonusType.Health, "生命"],
        [BonusType.Thin, "减肥"],
        [BonusType.Speed, "移速+5"],
        [BonusType.Speed20, "移速+20"],
        [BonusType.BladeCount, "刀数"],
        [BonusType.BladeCount3, "刀数+3"],
        [BonusType.BladeLength, "刀长+5"],
        [BonusType.BladeLength20, "刀长+20"],
        [BonusType.BladeDamage, "刀伤"],
        [BonusType.BladeSpeed, "刀速+5"],
        [BonusType.BladeSpeed20, "刀速+20"],
        [BonusType.Random, "随机"]
    ]);

    public static toDisplayString(bonusType: BonusType): string {
        const name = this.bonusTypeToNameMap.get(bonusType);
        if (name !== undefined) {
            return name;
        }
        throw new Error(`Invalid BonusType: ${bonusType}`);
    }
}

// Define the structure for pickable bonuses in the game
type BonusDto = {
    t: BonusType;  // Type of the bonus
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