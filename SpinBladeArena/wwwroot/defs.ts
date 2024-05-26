type PlayerDto = {
    userId: number;
    userName: string;
    position: number[];
    destination: number[];
    health: number;
    size: number;
    blades: PlayerBladesDto[];
    score: number;
};

type PlayerBladesDto = {
    angle: number;
    damage: number;
    length: number;
};

type PickableBonusDto = {
    name: string;
    position: number[];
};

// Define the structure for player data
type PlayerDtoRaw = {
    u: number;  // Unique user ID
    n: string;  // Username
    s: number;  // Score of the player
    p: number[];  // Current position in the game world, usually an array of x, y, z coordinates
    d: number[];  // Destination position in the game world, similar to 'p'
    h: number;  // Health level of the player
    i: number;  // Size of the player
    b: BladeDtoRaw[];  // Array of blades the player has
};

// Define the structure for pickable bonuses in the game
type PickableBonusDtoRaw = {
    n: string;  // Name of the bonus
    p: number[];  // Position where the bonus is located, typically x, y, z coordinates
};

// Define the structure for a blade, part of a player's weaponry
type BladeDtoRaw = {
    a: number;  // Angle at which the blade is being held or used
    l: number;  // Length of the blade
    d: number;  // Damage potential of the blade
};

type PushStateDto = {
    i: number; // frame index
    p: PlayerDtoRaw[]; // Array of player data
    b: PickableBonusDtoRaw[]; // Array of pickable bonus data
    d: PlayerDtoRaw[]; // Array of dead player data
};

/**
* Converts the raw player data (PlayerDtoRaw) into a more readable format (PlayerDto)
* @param rawPlayer The raw player data
* @returns The formatted player data
*/
function convertPlayerDto(rawPlayer: PlayerDtoRaw): PlayerDto {
    return {
        userId: rawPlayer.u,
        userName: rawPlayer.n,
        position: rawPlayer.p,
        destination: rawPlayer.d,
        health: rawPlayer.h,
        size: rawPlayer.i,
        blades: rawPlayer.b.map(convertBladeDto),
        score: rawPlayer.s,
    };

    /**
     * Converts the raw blade data (BladeDtoRaw) into a more readable format (PlayerBladesDto)
     * @param rawBlade The raw blade data
     * @returns The formatted blade data
     */
    function convertBladeDto(rawBlade: BladeDtoRaw): PlayerBladesDto {
        return {
            angle: rawBlade.a,
            length: rawBlade.l,
            damage: rawBlade.d,
        };
    }
}

/**
 * Converts the raw pickable bonus data (PickableBonusDtoRaw) into a more readable format (PickableBonusDto)
 * @param rawBonus The raw pickable bonus data
 * @returns The formatted pickable bonus data
 */
function convertPickableBonusDto(rawBonus: PickableBonusDtoRaw): PickableBonusDto {
    return {
        name: rawBonus.n,
        position: rawBonus.p,
    };
}

declare var initState: PushStateDto;