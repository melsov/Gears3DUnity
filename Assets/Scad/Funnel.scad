union() {
    rad = 1;
    height = 1.5;
    _fn = 8;
    cylinder($fn=_fn, h=height, r1=rad, r2 =rad * 3.2);
    translate([0,0,-height/2])
    cylinder($fn=_fn,h=height/2, r=rad);
}