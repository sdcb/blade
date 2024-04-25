type PlayerDto = {
    userId: number;
    userName: string;
    position: number[];
    health: number;
    size: number;
    blades: PlayerBladesDto;
    deadTime: number;
};

type PlayerBladesDto = {
    length: number;
    damage: number;
    blades: PlayerBladeInfoDto[];
};

type PlayerBladeInfoDto = {
    rotationAngle: number;
};

type PickableBonusDto = {
    name: string;
    position: number[];
};