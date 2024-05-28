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
        return Player.minSize + this.health;
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