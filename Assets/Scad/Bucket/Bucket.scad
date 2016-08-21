
h=10;
r=15;
ysc=2.5;
res = 32;
d=1;
//grabber
union() {
    difference() {
        scale([2,ysc,1]) {
            difference() {
                cyl(h=h,r=r,center=true);
                scale([.5,1,1]) {
                    translate([0, r, 0]) {
                        cyl(h=h*2,r=r*1.8,center=true);
                    }
                }
             
                translate([0,r * .8,0])
                    cube([r*4, r , r*10], true);   
            }
        }
        //remove half 
        translate([-r*15*(1 - d), -r*15, -r*15])
            cube([r*30,r*30,r*30],false);
    }
    //cylinder base
    translate([0, -r * ysc, h * .4 * d])
        cylinder($fn = 6, h=h * .6, r1 = 2, r2 = 2, center = true);
//        cyl(h=h * .6,r=2,center=true);
}

module cyl(h=10, r=15, center=true){
    cylinder($fn = res, h=h, r1 = r, r2 = r, center = true);
}