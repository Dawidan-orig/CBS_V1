using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CBS.Modules
{
    class BattleModule : BaseCommandModule //TODO
    {
        //TODO Случайные встречи
        //TODO Управление реакциями
        //TODO Взаимосвязь с профилем
        //TODO "Подземелья"
        class Mobs {
            private class Mob
            {
                int health;
                int damage;
                string name;
                Mob(string name, int health, int damage) 
                {
                    this.name = name;
                    this.damage = damage;
                    this.health = health;
                }

                //Модификаторы
                float critMod=1; //На исходящий урон. Зависит от удачи (0;+беск)
                float willPower=1; //на здоровье. Зависит от удачи (0;+беск)
                float armor=0; //На входяший урон [0,1)
                float luck=1; //На разброс random'а (0;2)
            }
        }

        [Command("battle")] 
        [Description("не (пока что) начинает (пока что) битву с чем-то")]
        public async Task Battlemode(CommandContext ctx)
        {
            throw new NotImplementedException();
        }
    }
}
