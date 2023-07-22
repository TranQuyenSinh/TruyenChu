var gulp = require("gulp"),
    cssmin = require("gulp-cssmin");
rename = require("gulp-rename");
const sass = require("gulp-sass")(require("sass"));

gulp.task("sass", function () {
    return gulp
        .src("assets/scss/main.scss")
        .pipe(sass().on("error", sass.logError))
        .pipe(cssmin())
        .pipe(
            rename({
                suffix: ".min",
            })
        )
        .pipe(gulp.dest("wwwroot/css/"));
});

gulp.task('sassadmin', function () {
    return gulp
        .src('assets/scss/admin/main.scss')
        .pipe(sass().on('error', sass.logError))
        .pipe(cssmin())
        .pipe(
            rename({
                suffix: '.admin.min',
            })
        )
        .pipe(gulp.dest('wwwroot/css/'))
})

gulp.task(
    "default",
    gulp.series("sass", function (cb) {
        gulp.watch("assets/scss/*.scss", gulp.series("sass"));
        gulp.watch('assets/scss/admin/*.scss', gulp.series('sassadmin'))
        cb();
    })
);


