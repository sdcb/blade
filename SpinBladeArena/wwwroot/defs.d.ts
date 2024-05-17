declare type PlayerDto = {
    userId: number;
    userName: string;
    position: number[];
    destination: number[];
    health: number;
    size: number;
    blades: PlayerBladesDto[];
    score: number;
};

declare type PlayerBladesDto = {
    angle: number;
    damage: number;
    length: number;
};

declare type PickableBonusDto = {
    name: string;
    position: number[];
};