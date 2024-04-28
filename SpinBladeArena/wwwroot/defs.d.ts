type PlayerDto = {
    userId: number;
    userName: string;
    position: number[];
    destination: number[];
    health: number;
    size: number;
    blades: PlayerBladesDto;
    deadTime: number;
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