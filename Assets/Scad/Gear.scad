
TOOTH_MODE_HULL = 0;
TOOTH_MODE_MINKOWSKI = 1;
TOOTH_MODE_SQUARE = 2;

tMode = 1; // TOOTH_MODE_HULL;
scale_ = 1;
toothWidth = .25;

//Example
gearT(6, toothWidth, .5, .25, tMode, scale_);

//Generates a gear mesh
//Radius scales with tooth width (twidth) and toothCount
module gearT(toothCount, twidth, height, tdepth, toothMode = TOOTH_MODE_HULL, _scale = 1) 
{
t = toothCount;
twidth = twidth * _scale;
innerR = twidth/sin(180/t);
hi = height * _scale;
ir = innerR;
tdepth = tdepth * _scale;
or = innerR + tdepth;
wedge = 360 / (t);
echo(or);

union(){
    rotate([0,0,.25 * wedge])translate([0,0,0])
        cylinder($fn = t * 2, hi, ir, ir, true);
    for(i = [0 : floor(t)]) {
        rotate([0, 0, (i + 0) * wedge]) {
            $fn = 30;
            if (toothMode == 0) {
                translate([0,0,-hi/2])
                hull() {
                   outerCenter = or - tdepth/2;
                   toothBaseWidth = twidth * 1;
                   innerCenter = ir - toothBaseWidth / 2;
                   translate([0,outerCenter,0]) 
                        cylinder(h = hi, r = twidth/2);
                   translate([0,innerCenter,0]) 
                        cylinder(h = hi, r = twidth/(1.6));
                 }
            } else if (toothMode == 1) {
                translate([-twidth/4,0,0])
                minkowski() {
                    cube([twidth/2, or , hi/2], true);
                    translate([twidth/4.0, or/2,-hi/4.0]) 
                        cylinder(h = hi/2, r=twidth/4.0);
                }
            } else {
                cube([twidth, or * 2 , hi], true);
            }
        }
    }
}
}

//tooth width scales with tooth count and inner radius
module gear(teeth, height, innerR, outerR) {
t = teeth;
hi = height;
ir = innerR;
or = outerR;

wedge = 360 / (t);
twidth = ir * sin (wedge / 2.0);
echo(twidth);

$fn = 30;
union(){
    cylinder($fn = t * 2, hi, ir, ir, true);
    for(i = [0 : t]) {
        rotate([0, 0, (i + .25) * wedge]) {
            translate([-twidth/4,0,0])
            minkowski() {
                cube([twidth/2 * 1.05, or , hi/2], true);
                translate([twidth/4.0, or/2,-hi/4.0]) 
                    cylinder(h = hi/2, r=twidth/4.0);
            }
        }
    }
}
}