
TOOTH_MODE_HULL = 0;
TOOTH_MODE_MINKOWSKI = 1;
TOOTH_MODE_SQUARE = 2;

tMode = 0; // TOOTH_MODE_HULL;
scale_ = 1;
toothWidth = .25;
toothCount = 16;
height = .5;
toothDepth = .25;

//peg();
//Example
sawTooth(toothCount, toothWidth, height, toothDepth, toothDepth * 2.7, tMode, scale_);
//gearT(toothCount, toothWidth, height, toothDepth, tMode, scale_);

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

rotate([0,0,90]) {
difference() {
union(){
    rotate([0,0,.25 * wedge])translate([0,0,0])
        cylinder($fn = t * 2, hi, ir, ir, true);
    for(i = [0 : floor(t)]) {
        rotate([0, 0, (i + 0) * wedge]) {
            $fn = 30;
            if (toothMode == 0) {
                translate([0,0,-hi/2])
                hullTooth(or, innerR, toothDepth, twidth, hi);            
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
//holes
color([0,.7,1,1]) {
   translate([0,0,-hi]) {
       centerSocketR = twidth * .85;
       holeR = twidth *.65;
       if (t < 8) {
           centerSocketR = twidth * .65;
           holeR = twidth *.45;
           cylinder($fn = 6, h = hi * 5, r = centerSocketR, true);
           socketHoles(radius=holeR, distanceFromCenter=innerR * .65);
       } else {
           cylinder($fn = 6, h = hi * 5, r = centerSocketR, true);
           socketHoles(radius=holeR, distanceFromCenter=innerR * .65);
       }
   }
}
}
}
}

module sawTooth(toothCount, twidth, height, tdepth, toothBaseOffset, toothMode = TOOTH_MODE_HULL, _scale = 1) 
{
t = toothCount;
twidth = twidth * _scale;
////innerR = toothBaseOffset; // twidth/sin(180/t);
hi = height * _scale;
ir = toothBaseOffset;
tdepth = tdepth * _scale;
or = toothBaseOffset + tdepth;
wedge = 360 / (t);

start = twidth * 2 * t;
boxStart = start + twidth * 1.2;
rotate([90,0,0]) {
difference() {
union(){
    
    translate([-boxStart/2, 0, -hi/2])
    cube([boxStart, toothBaseOffset, hi], false);
    translate([-start * .5, 0, 0])
    for(i = [0 : t]) {
        translate([i * twidth * 2, toothDepth /2, 0]) {
            $fn = 30;
            if (toothMode == 0) {
                translate([0,0,-hi/2])
                hullTooth(or, toothBaseOffset, toothDepth, twidth, hi);            
            } 
            else if (toothMode == 1) {
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
//holes
color([0,.7,1,1]) {
   translate([0,toothBaseOffset/2,-hi]) {
       centerSocketR = twidth * .85;
       holeR = twidth *.65;
       translate([-start/2 + twidth, 0, 0]) {
        cylinder($fn = 6, h = hi * 5, r = centerSocketR, true);
        translate([twidth * 3,0,0])
           socketHole(holeR);
       }
       translate([start/2 - twidth, 0, 0]) {
        cylinder($fn = 6, h = hi * 5, r = centerSocketR, true);  
           translate([-twidth * 3,0,0])
           socketHole(holeR);
       }
   }
}
}
}
}

module hullTooth(or, ir, tdepth, twidth, hi) {
    hull() {
               outerCenter = or - tdepth/2;
               toothBaseWidth = twidth * 1;
               innerCenter = ir - toothBaseWidth / 2;
               translate([0,outerCenter,0]) 
                    cylinder(h = hi, r = twidth/2);
               translate([0,innerCenter,0]) 
                    cylinder(h = hi, r = twidth/(1.6));
             }
}

module peg() {
    color([0,.7,1,1]) {
       centerSocketR = toothWidth * .85;
       if (toothCount < 8) {
           centerSocketR = toothWidth * .65;
           cylinder($fn = 6, h = height * 5, r = centerSocketR, true);
       } else {
           cylinder($fn = 6, h = height * 5, r = centerSocketR, true);
       }
   }
}

module socketHoles(radius, distanceFromCenter) {
   sockets = 4;
   for(i = [0 : sockets]) {
       rad = distanceFromCenter;
       ang = (i/sockets) * 360;
       translate([rad * sin(ang), rad * cos (ang), 0]) {
           socketHole(radius);
//            cylinder($fn = 14, h = hi * 5, r = radius, true);       
        }
    }
}

module socketHole(radius) {
    cylinder($fn = 14, h = hi * 5, r = radius, true); 
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