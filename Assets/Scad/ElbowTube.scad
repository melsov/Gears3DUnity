module pipe(l,r) {
    assign(distance=l-2*r) {
        hull() {
            translate([0,0,0])
            sphere(r=r);
            translate([0,0,distance])
            sphere(r=r);
        }
    }
}

//translate([0,0,15]) 
//cube(10,true);

color([0, .8, .5]) {
    union() {
        pipe(l=20, r=5);
//        rotate([90,0,0])
//        pipe(l=20, r=5);
    }
}