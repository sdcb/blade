type PlayerDto = {
    userId: number;
    userName: string;
    position: number[];
    destination: number[];
    health: number;
    size: number;
    blades: PlayerBladesDto;
    score: number;
};

type PlayerBladesDto = {
    length: number;
    damage: number;
    angles: number[];
};

type PickableBonusDto = {
    name: string;
    position: number[];
};