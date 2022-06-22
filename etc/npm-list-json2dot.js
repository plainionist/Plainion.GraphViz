const fs = require('fs')

//
// usage: 
// run "npm list --json > deps.json" in your web project home 
// run "node <full-path-to-this-script> deps.json > deps.dot"
// load "deps.dot" into Plainion.GraphViz
//

const file = process.argv[2]
const json = JSON.parse(fs.readFileSync(file, 'utf8'))

function visit(source, deps) {
    Object.getOwnPropertyNames(deps).forEach( key => {
        console.log(`  "${source}" -> "${key}"`)

        if (deps[key].dependencies) {
            visit(key, deps[key].dependencies)
        }
    })
}


console.log('digraph {')

visit('PROJECT', json.dependencies)

console.log('}')


