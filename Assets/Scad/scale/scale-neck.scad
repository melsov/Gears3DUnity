h=1;
ct=12;
rad=15;
cr=5;
points = [[]];
faces = [[]];
union() {
    for(i = [0:ct]){
        ang = (i/ct)*180*.6;
        si=sin(ang);
        y = rad*si;
        z = h*i;
        cr = cr * max(si, .4);
        pl = [[0,0,z],[0,y,z],[cr,y,z]];
        points = concat(points,pl);
        if(i==0){
            faces=[[0,1,2]];
        } else if (i < ct) {
            ri0=i*3-3;
            ri1=ri0+1;
            ri2=ri0+2;
            ii0=i*3;
            ii1=ii0+1;
            ii2=ii0+2;
            faceAdd=[[ri0,ri1,ii1],[ri1,ri2,ii1],[ri2,ri0,ii0]];
            faces = concat(faces,faceAdd);
        }
        translate([0,y,z]) {
            cylinder($fn=3,h=h,r1=cr,r2=cr,center=true);
        }
        
        translate([0,-y,z]) {
            cylinder($fn=3,h=h,r1=cr,r2=cr,center=true);
        }
    }
}

//echo(points);
//polyhedron(points=points, faces=faces);